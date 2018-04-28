module App.Types

open Chiron

type Model = 
    {
        currentPage: string
        chat: Chat.Types.ChatState
    }

    static member FromJson (_: Model) = json {
        let! p = Json.read "currentPage"
        return { currentPage = p; chat = Chat.Types.NotConnected }
    }

    static member ToJson (x: Model) = json {
        do! Json.write "currentPage" x.currentPage
    }
