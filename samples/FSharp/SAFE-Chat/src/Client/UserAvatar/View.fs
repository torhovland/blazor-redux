module UserAvatar.View

open Fable.Helpers.React
open Fable.Helpers.React.Props

let root  =
  function
  | None | Some "" ->
      div [ ClassName "fs-avatar" ] []

  | Some url ->
      div
          [ ClassName "fs-avatar"
            Style [BackgroundImage (sprintf "url(%s)" url) ] ]
          []
