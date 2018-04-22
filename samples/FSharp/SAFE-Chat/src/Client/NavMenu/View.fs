module NavMenu.View

open Fable.Core.JsInterop
open Fable.Helpers.React
open Props

open Router
open Channel.Types
open Chat.Types
open Fable.Import

let menuItem htmlProp name topic isCurrent =
    button
      [ classList [ "btn", true; "fs-channel", true; "selected", isCurrent ]
        htmlProp ]
      [ h1 [] [str name]
        span [] [str topic]]

let menuItemChannel (ch: ChannelInfo) currentPage = 
    let targetRoute = Channel ch.Id
    let jump _ = Browser.location.hash <- toHash targetRoute
    menuItem (OnClick jump) ch.Name ch.Topic (targetRoute = currentPage)

let menuItemChannelJoin dispatch = 
    let join chid _ = chid |> Join |> dispatch
    fun (ch: ChannelInfo) ->
      menuItem (OnClick <| join ch.Id) ch.Name ch.Topic false

let menu (chatData: ChatState) currentPage dispatch =
    match chatData with
    | NotConnected ->
      [ div [] [str "not connected"] ]
    | Connected (me, chat) ->
      let opened, newChanName = chat.NewChanName |> function |Some text -> (true, text) |None -> (false, "")
      [ yield div
          [ ClassName "fs-user" ]
          [ UserAvatar.View.root me.ImageUrl
            h3 [Id "usernick"] [str me.Nick]
            span [Id "userstatus"] [ str me.Status]
            button
              [ Id "logout"; ClassName "btn"; Title "Logout"
                OnClick (fun _ -> Browser.location.href <- "/logoff") ]
              [ i [ ClassName "mdi mdi-logout-variant"] [] ]
           ]
        yield h2 []
          [ str "My Channels"
            button
              [ ClassName "btn"; Title "Create New"
                OnClick (fun _ -> (if opened then None else Some "") |> (SetNewChanName >> dispatch)) ]
              [ i [ classList [ "mdi", true; "mdi-close", opened; "mdi-plus", not opened ] ] []]
          ]
        yield input
          [ Type "text"
            classList ["fs-new-channel", true; "open", opened]
            Placeholder "Type the channel name here..."
            DefaultValue newChanName
            AutoFocus true
            OnChange (fun ev -> !!ev.target?value |> (Some >> SetNewChanName >> dispatch) )
            OnKeyPress (fun ev -> if !!ev.which = 13 || !!ev.keyCode = 13 then dispatch CreateJoin)
            ]

        for (_, ch) in chat.Channels |> Map.toSeq do
          yield menuItemChannel ch.Info currentPage

        yield h2 []
            [ str "All Channels"
              button
                [ ClassName "btn"; Title "Search" ]
                [ i [ ClassName "mdi mdi-magnify" ] []]
            ]
        for (chid, ch) in chat.ChannelList |> Map.toSeq do
            if chat.Channels |> Map.containsKey chid |> not then
                yield menuItemChannelJoin dispatch ch
      ]
