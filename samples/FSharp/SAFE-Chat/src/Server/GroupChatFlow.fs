module GroupChatFlow

open Akka.Actor
open Akkling

open Suave.Logging

open ChatTypes

type ChannelConfig = {
    // instructs channel actor to shutdown channel after last member exited
    autoRemove: bool
} with static member Default = {autoRemove = false}

module internal Internals =
    // maps user login
    type ChannelParties = Map<UserId, ClientMessage IActorRef>

    type ChannelState = {
        Config: ChannelConfig
        Parties: ChannelParties
        LastEventId: int
    }

    let logger = Log.create "chanflow"

open Internals

let createActor<'User, 'Message when 'User: comparison> (system: IActorRefFactory) (config: ChannelConfig) =

    let incId chan = { chan with LastEventId = chan.LastEventId + 1}
    let dispatch (parties: ChannelParties) (msg: ClientMessage): unit =
        parties |> Map.iter (fun _ subscriber -> subscriber <! msg)
    let allMembers = Map.toSeq >> Seq.map fst

    let rec behavior state (ctx: Actor<_>) =
        let updateState newState = become (behavior newState ctx) in
        let ts = state.LastEventId, System.DateTime.Now
        function
        | NewParticipant (user, subscriber) ->
            logger.debug (Message.eventX "NewParticipant {user}" >> Message.setFieldValue "user" user)
            let parties = state.Parties |> Map.add user subscriber
            do dispatch state.Parties <| Joined (ts, user, parties |> allMembers)
            incId { state with Parties = parties} |> updateState

        | ParticipantLeft user ->
            logger.debug (Message.eventX "Participant left {user}" >> Message.setFieldValue "user" user)
            let parties = state.Parties |> Map.remove user
            do dispatch state.Parties <| Left (ts, user, parties |> allMembers)

            if config.autoRemove && parties |> Map.isEmpty then
                logger.debug (Message.eventX "It was the last participant, closing the channel")
                stop()
            else
                incId { state with Parties = parties} |> updateState

        | ParticipantUpdate user ->
            logger.debug (Message.eventX "Participant updated {user}" >> Message.setFieldValue "user" user)
            do dispatch state.Parties <| Updated (ts, user)
            ignored state

        | NewMessage (user, message) ->
            if state.Parties |> Map.containsKey user then
                do dispatch state.Parties <| ChatMessage (ts, user, message)
            incId state |> updateState

        | ListUsers ->
            let users = state.Parties |> Map.toList |> List.map fst
            ctx.Sender() <! users
            ignored state

    in
    props <| actorOf2 (behavior { Parties = Map.empty; LastEventId = 1000; Config = config }) |> (spawn system null)
