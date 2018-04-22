module Logon

open Suave
open Suave.Html

open ChatUser

type ClientSession = NoSession | UserLoggedOn of RegisteredUser

module Views =
    let private partUser (session : ClientSession) = 
        div ["id", "part-user"] [
            match session with
            | UserLoggedOn user ->
                yield p [] [Text (sprintf "Logged on as %s" (getUserNick user))]
                yield p [][]
                yield a "/" [] [Text "Proceed to chat screen"]
                yield p [][]
                yield Text "Or you can "
                yield a "/logoff" [] [Text "log off now"]
                yield p [][]
            | _ ->
                yield tag "form" ["method", "POST"] (
                    [ p ["class", "subtitle"]
                        [ Text "Log on using your "
                          a "/oaquery?provider=Google" [] [Text "Google"]
                          Text " or "
                          a "/oaquery?provider=Github" [] [Text "Github"]
                          Text " account, or..." ]
                      div ["class", "label"]
                        [ Text "Choose a nickname" ]
                      div ["class", "field"]
                        [ div ["class", "control"]
                            [ tag "input" ["id", "nickname"; "class","input"; "name", "nick"; "type", "text"; "required", "true"] [] ]]
                      div ["class", "control"]
                          [ tag "input" [
                              "id", "login"
                              "class", "button is-primary"
                              "type", "submit"; "value", "Connect anonymously"] [] ] ]
                )
        ]

    let page content =
        html []
            [ head []
                [ title [] "F# Chat server"
                  link [ "rel", "stylesheet"
                         "href", "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.6.1/css/bulma.css" ]
                  link [ "rel", "stylesheet"
                         "href", "logon.css" ] ]
              body []
                [ div ["id", "header"]
                    [ tag "h1" ["class", "title"] [Text "F# Chat server"]
                      tag "h1" ["class", "subtitle"] [Text "Logon screen"]
                      hr [] ]
                  content

                  tag "footer" ["class", "footer"]
                    [ div [ "class", "container"]
                        [ div [ "class", "content has-text-centered"]
                            [   tag "strong" [] [Text "F# Chat"]
                                Text " built by "
                                a "https://github.com/OlegZee" [] [Text "Anonymous"]
                                Text " with (in alphabetical order) "
                                a "http://getakka.net" [] [Text "Akka.NET"]
                                Text ", "
                                a "https://github.com/Horusiath/Akkling" [] [Text "Akkling"]
                                Text ", "
                                a "http://fable.io" [] [Text "Fable"]
                                Text ", "
                                a "http://ionide.io" [] [Text "Ionide"]
                                Text " and "
                                a "http://suave.io" [] [Text "Suave.IO"] ]
                        ]]
                ]]

    let index session = page (partUser session)
