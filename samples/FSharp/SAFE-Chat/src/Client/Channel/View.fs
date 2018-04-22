module Channel.View

open Fable.Core.JsInterop
open Fable.Helpers.React

open Props
open Types

open Fable.ReactMarkdownImport

let private formatTs (ts: System.DateTime) =
  match (System.DateTime.Now - ts) with
  | diff when diff.TotalMinutes < 1.0 -> "a few seconds ago"
  | diff when diff.TotalMinutes < 30.0 -> sprintf "%i minutes ago" (int diff.TotalMinutes)
  | diff when diff.TotalHours <= 12.0 -> ts.ToShortTimeString()
  | diff when diff.TotalDays <= 5.0 -> sprintf "%i days ago" (int diff.TotalDays)
  | _ -> ts.ToShortDateString()

let inline valueOrDefault value =
    Ref <| (fun e -> if e |> isNull |> not && !!e?value <> !!value then e?value <- !!value)

let messageInput dispatch model =
  div
    [ ClassName "fs-message-input" ]
    [ input
        [ Type "text"
          Placeholder "Type the message here..."
          valueOrDefault model.PostText
          OnChange (fun ev -> !!ev.target?value |> (SetPostText >> dispatch))
          OnKeyPress (fun ev -> if !!ev.which = 13 || !!ev.keyCode = 13 then dispatch PostText)
        ]
      button
        [ ClassName "btn" ]
        [ i [ ClassName "mdi mdi-send mdi-24px"
              OnClick (fun _ -> dispatch PostText) ] [] ]
    ]

let chanUsers (users: Map<string, UserInfo>) =
  let screenName (u: UserInfo) =
    match u.IsBot with |true -> sprintf "#%s" u.Nick |_ -> u.Nick
  div [ ClassName "userlist" ]
      [ str "Users:"
        ul []
          [ for u in users ->
              li [] [str <| screenName u.Value]
          ]]

let chatInfo dispatch (model: ChannelData) =
  div
    [ ClassName "fs-chat-info" ]
    [ h1
        [] [ str model.Info.Name ]
      span
        [] [ str model.Info.Topic ]
      button
        [ Id "leaveChannel"
          ClassName "btn"
          Title "Leave"
          OnClick (fun _ -> dispatch Leave) ]
        [ i [ ClassName "mdi mdi-door-closed mdi-18px" ] []]
    ]

let message (text: string) =
    [ reactMarkdown [Source text] ]

let messageList (messages: Message Envelope list) =
    div
      [ ClassName "fs-messages" ]
      [ for m in messages ->
          match m.Content with
          | UserMessage (text, user) ->
              // Fable.Import.Browser.console.warn <| sprintf "%A %A" text user
              div
                [ classList ["fs-message", true; "user", user.isMe ] ]
                [ div
                    []
                    [ yield! message text
                      yield h5  []
                          [ span [ClassName "user"] [str user.Nick]
                            span [ClassName "time"] [str <| formatTs m.Ts ]] ]
                  UserAvatar.View.root user.ImageUrl
                ]

          | SystemMessage text ->
              blockquote
                [ ClassName ""]
                [ str text; str " "
                  small [] [str <| formatTs m.Ts] ]
      ]


let root (model: ChannelData) dispatch =
    [ chatInfo dispatch model
      div [ ClassName "fs-splitter" ] []
      messageList model.Messages
      div [ ClassName "fs-splitter" ] []
      messageInput dispatch model
     ]
