module UserSessionFlow

open System
open Akkling.Streams
open Akka.Streams
open Akka.Streams.Dsl

open Suave
open Suave.Logging

open ChatUser
open ChatTypes
open UserStore
open SocketFlow

open FsChat
open ProtocolConv

let logger = Log.create "chatapi"

type internal IncomingMessage =
    | ChannelMessage of ChannelId * Message
    | ControlMessage of Protocol.ServerMsg
    | Trash of reason: string

// extracts message from websocket reply, only handles User input (channel * string)
let internal extractMessage message =
    try
        match message with
        | Text t ->
            match t |> Json.unjson<Protocol.ServerMsg> with
            | Protocol.UserMessage {chan = channelId; text = messageText} ->
                match Int32.TryParse channelId with
                | true, chanId -> ChannelMessage (ChannelId chanId, Message messageText)
                | _ -> Trash "Bad channel id"
            | message -> ControlMessage message                
        | x -> Trash <| sprintf "Not a Text message '%A'" x
    with e ->
        do logger.error (Message.eventX "Failed to parse message '{msg}'. Reason: {e}" >> Message.setFieldValue "msg" message  >> Message.setFieldValue "e" e)
        Trash "exception"

let internal partitionFlows (pfn: _ -> int) worker1 worker2 combine =
    Akkling.Streams.Graph.create2 combine (fun b (w1: FlowShape<_,_>) (w2: FlowShape<_,_>) ->
        let partition = Partition<'TIn>(2, System.Func<_,int>(pfn)) |> b.Add
        let merge = Merge<'TOut> 2 |> b.Add

        b.From(partition.Out 0).Via(w1).To(merge.In 0) |> ignore
        b.From(partition.Out 1).Via(w2).To(merge.In 1) |> ignore

        FlowShape(partition.In, merge.Out)
    ) worker1 worker2
    |> Flow.FromGraph

/// User session multiplexer. Creates a flow that receives user messages for multiple channels, binds each stream to channel flow
/// and finally collects the messages from multiple channels into single stream.
/// When materialized return a "connect" function which, given channel and channel flow, adds it to session. "Connect" returns a killswitch to remove the channel.
let createMessageFlow
    (materializer: Akka.Streams.IMaterializer) =

    let inhub = BroadcastHub.Sink<ChannelId * Message>(bufferSize = 256)
    let outhub = MergeHub.Source<ChannelId * ClientMessage>(perProducerBufferSize = 16)

    let sourceTo (sink) (source: Source<'TOut, 'TMat>) = source.To(sink)

    let combine
            (producer: Source<ChannelId * Message, Akka.NotUsed>)
            (consumer: Sink<ChannelId * ClientMessage, Akka.NotUsed>)
            (chanId: ChannelId) (chanFlow: Flow<Message, ClientMessage, Akka.NotUsed>) =

        let infilter =
            Flow.empty<ChannelId * Message, Akka.NotUsed>
            |> Flow.filter (fst >> (=) chanId)
            |> Flow.map snd

        let graph =
            producer
            |> Source.viaMat (KillSwitches.Single()) Keep.right
            |> Source.via infilter
            |> Source.via chanFlow
            |> Source.map (fun message -> chanId, message)
            |> sourceTo consumer

        graph |> Graph.run materializer

    Flow.ofSinkAndSourceMat inhub combine outhub

let createSessionFlow (userStore: UserStore) messageFlow controlFlow =

    let extractChannelMessage (ChannelMessage (chan, message) | OtherwiseFail (chan, message)) = chan, message
    let extractControlMessage (ControlMessage message | OtherwiseFail message) = message

    let encodeChannelMessage (getUser: GetUser) channelId : ClientMessage -> Protocol.ClientMsg Async =
        let returnUserEvent (id, ts) userid f = async {
            let! userResult = getUser userid
            let user2userInfo = function
                | Some user -> mapUserToProtocol user
                | _ -> makeBlankUserInfo "zz" "unknown"
            return Protocol.ChannelEvent {id = id; ts = ts; evt = userResult |> (user2userInfo >> f)}
        }
        function
        | ChatMessage ((id, ts), UserId authorId, Message message) ->
            async.Return <| Protocol.ChanMsg {id = id; ts = ts; text = message; chan = channelId; author = authorId}
        | Joined (idts, userid, _) ->
            returnUserEvent idts userid (fun u -> Protocol.Joined (channelId, u))
        | Left ((id, ts), UserId userid, _) ->
            async.Return <| Protocol.ChannelEvent {id = id; ts = ts; evt = Protocol.Left (channelId, userid)}
        | Updated (idts, userid) ->
            returnUserEvent idts userid (fun u -> Protocol.Updated (channelId, u))

    let partition = function |ChannelMessage _ -> 0 | _ -> 1

    let userMessageFlow =
        Flow.empty<IncomingMessage, Akka.NotUsed>
        |> Flow.map extractChannelMessage
        // |> Flow.log "User flow"
        |> Flow.viaMat messageFlow Keep.right
        |> Flow.asyncMap 1 (fun (ChannelId channelId, message) -> encodeChannelMessage userStore.GetUser (channelId.ToString()) message)

    let controlFlow =
        Flow.empty<IncomingMessage, Akka.NotUsed>
        |> Flow.map extractControlMessage
        // |> Flow.log "Control flow"
        |> Flow.via controlFlow

    let combinedFlow : Flow<IncomingMessage,Protocol.ClientMsg,_> =
        partitionFlows partition userMessageFlow controlFlow Keep.left

    let socketFlow =
        Flow.empty<WsMessage, Akka.NotUsed>
        |> Flow.map extractMessage
        // |> Flow.log "Extracting message"
        |> Flow.viaMat combinedFlow Keep.right
        |> Flow.map (Json.json >> Text)

    socketFlow
