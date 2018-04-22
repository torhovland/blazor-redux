module Channel.State

open Elmish
open Types
open Fable.Import

let init () : ChannelData * Cmd<Msg> =
    {Users = Map.empty; Messages = []; PostText = ""; Info = ChannelInfo.Empty}, Cmd.none

let init2 (chan: ChannelInfo, users: UserInfo list) : ChannelData * Cmd<Msg> =
    { (fst <| init()) with Info = chan; Users = users |> List.map (fun u -> u.Id, u) |> Map.ofList }, Cmd.none

let getUserNick userid users =
    users |> Map.tryFind userid |> Option.map (fun user -> user.Nick)

let unknownUser userId () = {
    Id = userId; Nick = "Unknown #" + userId; Status = ""
    IsBot = false; Online = true; ImageUrl = None; isMe = false}

let update (msg: Msg) state: (ChannelData * Msg Cmd) =

    match msg with
    | Init (info, userlist) ->
        { Info = info; Messages = []; PostText = ""
          Users = userlist |> List.map (fun u -> u.Id, u) |> Map.ofList}, Cmd.none
    | Update info ->
        { state with Info = info }, Cmd.none

    | AppendMessage message ->
        { state with Messages = state.Messages @ [message] }, Cmd.none

    | AppendUserMessage (userId, { Id = id; Ts = ts; Content = text}) ->
        let authorInfo =
            state.Users
            |> Map.tryFind userId
            |> Option.defaultWith (unknownUser userId)
        let message = { Id = id; Ts = ts; Content = UserMessage (text, authorInfo) }
        { state with Messages = state.Messages @ [message] }, Cmd.none

    | UserJoined user ->
        let systemMessage = {
            Id = 0; Ts = System.DateTime.Now
            Content = SystemMessage <| sprintf "%s joined the channel" user.Nick }

        { state with
            Messages = state.Messages @ [systemMessage]
            Users = state.Users |> Map.add user.Id user}, Cmd.none

    | UserUpdated user ->
        let appendMessage =
            state.Users |> getUserNick user.Id |> function
            | Some oldnick when oldnick <> user.Nick ->
                let txt = sprintf "%s is now known as %s" oldnick user.Nick
                [{  Id = 0; Ts = System.DateTime.Now; Content = SystemMessage txt }]
            | _ -> []

        { state with
            Messages = state.Messages @ appendMessage
            Users = state.Users |> Map.add user.Id user}, Cmd.none

    | UserLeft userId ->
        let appendMessage =
            state.Users |> getUserNick userId |> function
            | Some oldnick ->
                let txt = sprintf "%s left the channel" oldnick
                [{  Id = 0; Ts = System.DateTime.Now; Content = SystemMessage txt }]
            | _ -> []
        { state with
            Messages = state.Messages @ appendMessage
            Users = state.Users |> Map.remove userId}, Cmd.none

    | SetPostText text ->
        {state with PostText = text}, Cmd.none

    | PostText ->
        match state.PostText with
        | text when text.Trim() <> "" ->
            {state with PostText = ""}, Cmd.ofMsg (Forward text)
        | _ ->
            state, Cmd.none

    | Leave
    | Forward _ ->
        Browser.console.error <| sprintf "%A message is not expected in channel update." msg
        state, Cmd.none
