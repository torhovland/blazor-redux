module Diag

open Akka.Actor
open Akkling

open ChatUser
open ChatTypes
open ChatServer

/// Creates an actor for echo bot.
let createEchoActor (getUser: GetUser) (system: ActorSystem) (botUserId: UserId) =

    let getPersonNick (RegisteredUser (_, user)) =
        match user with
        |Person { nick = nickName }
        |Anonymous { nick = nickName }
            -> Some nickName
        | _ -> None        

    let forUser userid fn = async {
        let! user = getUser userid
        return user |> Option.bind getPersonNick |> Option.map fn
    }

    let handler (ctx: Actor<_>) =
        let rec loop () = actor {
            let! msg = ctx.Receive()
            let! reply =
                match msg with
                | ChatMessage (_, userid, Message message) ->
                    forUser userid (fun nickName -> sprintf "%s said: %s" nickName message)
                | Joined (_, userid, _) ->
                    forUser userid (fun nickName -> sprintf "Welcome aboard, %s!" nickName)
                | _ -> async.Return None

            match reply with
            | Some reply -> do ctx.Sender() <! NewMessage (botUserId, Message reply)
            | _ -> ()

            return! loop()
        }
        loop()
    in
    spawn system "echobot" <| props(handler)

let createDiagChannel (getUser: GetUser) (system: ActorSystem) (server: IActorRef<_>) (echoUserId, channelName, topic) =
    async {
        let bot = createEchoActor getUser system echoUserId

        let! result = server |> addChannel channelName topic None
        match result with
        | Ok chan ->
            chan.channelActor <! (NewParticipant (echoUserId, bot))
        | Error _ ->
            () // FIXME log error

    }
