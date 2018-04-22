module App.State

open Elmish
open Elmish.Browser.Navigation
open Fable.Websockets.Elmish
open Router
open Types

let urlUpdate (result: Option<Route>) model =
    match result with
    | None ->
        // console.error("Error parsing url")
        model, Navigation.modifyUrl  "#" // no matching route - go home
        // model,Navigation.modifyUrl (toHash model.currentPage)
    | Some route ->
        { model with currentPage = route }, []

let init result =
    let (chinfo, chinfoCmd) = Chat.State.init()
    let (model, cmd) = urlUpdate result { currentPage = Overview; chat = chinfo }
    model, Cmd.batch [ cmd
                       Cmd.map (ChatDataMsg) chinfoCmd
                       ]

let update msg model =
    match msg with
    | ChatDataMsg msg ->
        let (chinfo, chinfoCmd) = Chat.State.update msg model.chat
        { model with chat = chinfo }, Cmd.map ChatDataMsg chinfoCmd
