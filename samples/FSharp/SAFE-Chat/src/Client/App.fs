module App.View

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Websockets.Elmish
open Fable.Core.JsInterop

open Types
open App.State
open Router
open Chat.Types

open Fable.Helpers.React
open Props

importAll "./sass/app.scss"

let root model dispatch =

    let mainAreaView = function
        | Route.Overview -> [Overview.View.root]
        | Channel chan ->
            let toChannelMessage m = ChannelMsg (chan, m)

            match model.chat with
            | Connected (_,chatdata) when chatdata.Channels |> Map.containsKey chan ->

                Channel.View.root chatdata.Channels.[chan]
                  (toChannelMessage >> ApplicationMsg >> ChatDataMsg >> dispatch)

            | _ ->
                [div [] [str "bad channel route" ]]

    div
      [ ClassName "container" ]
      [ div
          [ ClassName "col-md-4 fs-menu" ]
          (NavMenu.View.menu model.chat model.currentPage (ApplicationMsg >> ChatDataMsg >> dispatch))
        div
          [ ClassName "col-xs-12 col-md-8 fs-chat" ]
          (mainAreaView model.currentPage) ]

open Elmish.React
open Elmish.Debug
open Elmish.HMR

// App
Program.mkProgram init update root
|> Program.toNavigable (parseHash Router.route) urlUpdate
#if DEBUG
|> Program.withDebugger
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
|> Program.run
