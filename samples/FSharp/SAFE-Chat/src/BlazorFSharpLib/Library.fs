namespace BlazorFSharpLib

open Chiron
open BlazorRedux
open App.Types
open Microsoft.AspNetCore.Blazor.Components

type ChatAppComponent() =
    inherit ReduxComponent<Model, Chat.Types.Msg>()

type SocketInterop() =
    static member val Store : Store<Model, Chat.Types.Msg> = null with get, set

    static member MessageFromJs str = 
        printfn "Received message: %s" str
        SocketInterop.Store.Dispatch (Chat.Types.Msg.ServerMessage str)

module MyFuncs =
    open FsChat

    let StateSerializer (state: Model) =
        state
        |> Json.serialize
        |> Json.format

    let StateDeserializer str : Model =
        str
        |> Json.parse
        |> Json.deserialize

    let ClientMsgDeserializer str : Protocol.ClientMsg =
        str
        |> Json.parse
        |> Json.deserialize

    let MyReducer state (action: Chat.Types.Msg) =
        match action with
            | Chat.Types.Msg.CreateJoin -> state
            | Chat.Types.Msg.ServerMessage str ->
                let m = ClientMsgDeserializer str                
                match m with
                    | Protocol.ClientMsg.Hello h ->
                        printfn "Reducing Hello message"
                        state
            | _ -> state

    let LocationReducer state (action: NewLocationAction) =
        { state with currentPage = action.Location }
