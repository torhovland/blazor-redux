module SocketFlow

open System.Text

open Akka.Actor
open Akka.Streams
open Akka.Streams.Dsl
open Akkling
open Akkling.Streams

open Suave.Logging
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

type WsMessage =
    | Text of string
    | Data of byte array
    | Ignore

let private logger = Log.create "socketflow"

// Provides websocket handshaking. Connects web socket to a pair of Source and Sync.
// 'materialize'
let handleWebsocketMessages (system: ActorSystem)
    (materialize: IMaterializer -> Source<WsMessage, Akka.NotUsed> -> Sink<WsMessage, _> -> unit) (ws : WebSocket) _
    =
    let materializer = system.Materializer()
    let sourceActor, inputSource =
        Source.actorRef OverflowStrategy.Fail 1000 |> Source.toMat Sink.publisher Keep.both
        |> Graph.run materializer |> fun (actor, pub) -> actor, Source.FromPublisher pub

    let emptyData = ByteSegment [||]

    let asyncMap f v = async { let! x = v in return f x }
    let asyncIgnore = asyncMap (fun _ -> Ignore)

    // sink for flow that sends messages to websocket
    let sinkBehavior _ (ctx: Actor<_>): WsMessage -> _ =
        function
        | Text text ->
            // using pipeTo operator just to wait for async send operation to complete
            ws.send Opcode.Text (Encoding.UTF8.GetBytes(text) |> ByteSegment) true
                |> asyncIgnore |!> ctx.Self
        | Data bytes ->
            ws.send Binary (ByteSegment bytes) true |> asyncIgnore |!> ctx.Self
        | Ignore -> ()
        >> ignored

    let sinkActor =
        props <| actorOf2 (sinkBehavior ()) |> (spawn system null) |> retype

    let sink: Sink<WsMessage,_> = Sink.ActorRef(untyped sinkActor, PoisonPill.Instance)
    do materialize materializer inputSource sink

    socket {
        let mutable loop = true
        while loop do
            let! msg = ws.read()
            
            match msg with
            | (Opcode.Text, data, true) -> 
                let str = Encoding.UTF8.GetString data
                sourceActor <! Text str
            | (Ping, _, _) ->
                do! ws.send Pong emptyData true
            | (Close, _, _) ->
                logger.debug (Message.eventX "Received Opcode.Close, terminating actor")
                (retype sourceActor) <! PoisonPill.Instance

                do! ws.send Close emptyData true
                // this finalizes the Source
                loop <- false
            | _ -> ()
    }

/// Creates Suave socket handshaking handler
let handleWebsocketMessagesFlow  (system: ActorSystem) (handler: Flow<WsMessage, WsMessage, Akka.NotUsed>) (ws : WebSocket) =
    let materialize materializer inputSource sink =
        inputSource |> Source.via handler |> Source.runWith materializer sink |> ignore
    handleWebsocketMessages system materialize ws
