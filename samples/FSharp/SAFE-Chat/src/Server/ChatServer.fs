module ChatServer

open System

open Akka.Actor
open Akkling
open Suave.Logging

open ChatTypes

let private logger = Log.create "chatserver"
type ChannelActor = ChannelMessage IActorRef

/// Channel is a primary store for channel info and data
type ChannelData = {
    id: ChannelId
    name: string
    topic: string
    channelActor: ChannelActor
}
and UserSessionData = {
    notifySink: ServerNotifyMessage IActorRef
}

and ServerData = {
    channels: ChannelData list
    sessions: Map<UserId, UserSessionData>
}

// notification message sent to a subscribers via notify method
and ServerNotifyMessage =
    | AddChannel of ChannelData
    | DropChannel of ChannelData

/// Server protocol
type ServerControlMessage =
    | UpdateState of (ServerData -> ServerData)
    | FindChannel of (ChannelData -> bool)
    | GetOrCreateChannel of name: string * topic: string * (IActorRefFactory -> ChannelActor)
    | ListChannels of (ChannelData -> bool)

    | StartSession of UserId * IActorRef<ServerNotifyMessage>
    | CloseSession of UserId

type ServerReplyMessage =
    | Done
    | RequestError of string
    | FoundChannel of ChannelData
    | FoundChannels of ChannelData list

type ServerT = IActorRef<ServerControlMessage>

let private initialState = { channels = []; sessions = Map.empty }

module internal Helpers =

    let updateChannel f chanId serverState: ServerData =
        let f chan = if chan.id = chanId then f chan else chan
        in
        {serverState with channels = serverState.channels |> List.map f}

    let byChanName name c = (c:ChannelData).name = name

    // verifies the name is correct
    let isValidName (name: string) =
        (String.length name) > 0 && Char.IsLetter name.[0]

    let __lastid = ref 100
    let newId () = System.Threading.Interlocked.Increment __lastid

module private Implementation =
    open Helpers

    /// Creates a new channel or returns existing if channel already exists
    let addChannel createChannel name topic (state: ServerData) =
        match state.channels |> List.tryFind (byChanName name) with
        | Some chan ->
            Ok (state, chan)
        | _ when isValidName name ->
            let channelActor = createChannel ()
            let newChan = {id = ChannelId (newId()); name = name; topic = topic; channelActor = channelActor }

            do state.sessions |> Map.iter(fun _ session -> session.notifySink <! AddChannel newChan)
            Ok ({state with channels = newChan::state.channels}, newChan)
        | _ ->
            Result.Error "Invalid channel name"

    let setTopic chanId newTopic state =
        Ok (state |> updateChannel (fun chan -> {chan with topic = newTopic}) chanId)

    let getChannelImpl message (server: ServerT) =
        async {
            let! (reply: ServerReplyMessage) = server <? message
            match reply with
            | FoundChannel channel -> return Ok channel
            | RequestError error -> return Result.Error error
            | _ -> return Result.Error "Unknown reason"
        }

let startServer (system: ActorSystem) : IActorRef<ServerControlMessage> =

    let rec serverBehavior (state: ServerData) (ctx: Actor<obj>): obj -> Effect<_> =
        let replyAndUpdate f = function
            | Ok (newState, reply) -> ctx.Sender() <! f reply; become (serverBehavior newState ctx)
            | Result.Error errtext -> ctx.Sender() <! RequestError errtext; ignored ()

        function
        | Terminated(ref, _, _) ->
            state.channels |> List.tryFind (fun chan -> chan.channelActor = ref) |>
            function
            | Some channel ->
                do state.sessions |> Map.iter(fun _ session -> session.notifySink <! DropChannel channel)
                become (serverBehavior { state with channels = state.channels |> List.except [channel]} ctx)
            | _ ->
                do logger.error (Message.eventX "Failed to locate terminated object: {a}" >> Message.setFieldValue "a" ref)
                ignored state

        | :? ServerControlMessage as msg ->
            match msg with
            | UpdateState updater ->
                become (serverBehavior (updater state) ctx)
            | FindChannel criteria ->
                let found = state.channels |> List.tryFind criteria
                ctx.Sender() <! (found |> function |Some chan -> FoundChannel chan |_ -> RequestError "Not found")
                ignored ()

            | GetOrCreateChannel (name, topic, createChannel) ->
                let createChannel1 () =
                    let actor = createChannel (ctx)
                    do logger.debug (Message.eventX "Started watching {a}" >> Message.setFieldValue "a" name)
                    monitor ctx actor |> ignore
                    actor
                state
                    |> Implementation.addChannel createChannel1 name topic
                    |> replyAndUpdate FoundChannel

            | ListChannels criteria ->
                let found = state.channels |> List.filter criteria
                ctx.Sender() <! FoundChannels found
                ignored()

            | StartSession (user, nsink) ->
                do logger.debug (Message.eventX "StartSession user={userId}" >> Message.setFieldValue "userId" user)

                let newState = { state with sessions = state.sessions |> Map.add user { notifySink = nsink } }
                become (serverBehavior newState ctx)

            | CloseSession userid ->
                do logger.debug (Message.eventX "CloseSession user={userId}" >> Message.setFieldValue "userId" userid)
                
                let newState = { state with sessions = state.sessions |> Map.remove userid }
                become (serverBehavior newState ctx)          
        | msg ->
            do logger.debug (Message.eventX "Failed to process message: {a}" >> Message.setFieldValue "a" msg)
            unhandled()
    in

    spawn system "ircserver" <| props (actorOf2 (serverBehavior initialState)) |> retype

let getChannel criteria =
    Implementation.getChannelImpl (FindChannel criteria)

let getOrCreateChannel name topic makeChan =
    Implementation.getChannelImpl (GetOrCreateChannel (name, topic, makeChan))

let listChannels criteria (server: ServerT) =
    async {
        let! (reply: ServerReplyMessage) = server <? (ListChannels criteria)
        match reply with
        | FoundChannels channels -> return Ok channels
        | _ -> return Result.Error "Unknown error"
    }

let startSession (server: ServerT) userId (actor: IActorRef<ServerNotifyMessage>) =
    server <! StartSession (userId, actor)

open GroupChatFlow  // TODO BAD dependency

let addChannel name topic (config: ChannelConfig option) (server: ServerT) = async {

    let createChannel = createActor >< (config |> Option.defaultValue ChannelConfig.Default)

    let! (reply: ServerReplyMessage) = server <? GetOrCreateChannel (name, topic, createChannel)
    return
        match reply with
        | FoundChannel channelData -> Ok channelData
        | RequestError message -> Result.Error message
        | _ -> Result.Error "Unknown reply"
}
