module App.Types

type Msg =
  | ChatDataMsg of Chat.Types.Msg

type Model = {
    currentPage: Router.Route
    chat: Chat.Types.ChatState
  }
