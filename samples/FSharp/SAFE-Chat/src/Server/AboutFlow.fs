module AboutFlow

open Akka.Actor
open Akkling

open Suave.Logging

open ChatTypes

let internal logger = Log.create "aboutflow"

let private aboutMessage =
    [   """## Welcome to F# Chat

F# Chat application built with Fable, Elmish, React, Suave, Akka.Streams, Akkling"""

        "Click on the channel name to join or click '+' and type in the name of the new channel."

        """Try the following commands in channel's input box:

* **/leave** - leaves the channel
* **/join <chan name>** - joins the channel, creates if it doesn't exist
* **/nick <newnick>** - changes your nickname
* **/status <newstatus>** - change status
* **/avatar <imageUrl>** - change user avatar
"""
]

let createActor systemUser (system: IActorRefFactory) =

    // TODO put user to map
    let mutable users = Map.empty

    let rec behavior (ctx: Actor<_>) =
        function
        | NewParticipant (user, subscriber) ->
            users <- users |> Map.add user subscriber
            logger.debug (Message.eventX "Sending about to {user}" >> Message.setFieldValue "user" user)
            let ts = (0, System.DateTime.Now)

            aboutMessage |> List.indexed |> List.iter (fun (i, m) ->
                ctx.System.Scheduler.ScheduleTellOnce( System.TimeSpan.FromMilliseconds(400. * float i), subscriber, ChatMessage (ts, systemUser, Message m))
                )
            // sending messages with some delay. Sending while flow is initialized causes intermittently dropped messages
            ignored ()

        | ParticipantLeft user ->
            users <- users |> Map.remove user
            logger.debug (Message.eventX "Participant left {user}" >> Message.setFieldValue "user" user)
            ignored ()

        | NewMessage (user, _) ->
            let ts = (0, System.DateTime.Now)
            let sub = users |> Map.find user
            do sub <! ChatMessage (ts, systemUser, Message "> Sorry, this feature is not implemented yet.")
            ignored ()

        | ListUsers ->
            do ctx.Sender() <! [systemUser]
            ignored ()

        | _ ->
            ignored ()

    in
    props <| actorOf2 behavior |> (spawn system null)

