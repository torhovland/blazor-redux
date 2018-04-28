namespace FsChat

open Chiron

[<RequireQualifiedAccess>]
module Protocol =

    type UserId = string
    type ChannelId = string

    type ChanUserInfo = 
        {
            id: UserId; nick: string; isbot: bool; status: string; email: string; imageUrl: string
        }

        static member FromJson (_: ChanUserInfo) = json {
            let! id = Json.read "id"
            let! nick = Json.read "nick"
            let! isbot = Json.read "isbot"
            let! status = Json.read "status"
            let! email = Json.read "email"
            let! imageUrl = Json.read "imageUrl"
            return { id = id; nick = nick; isbot = isbot; status = status; email = email; imageUrl = imageUrl }
        }

        static member ToJson (x: ChanUserInfo) = json {
            do! Json.write "id" x.id
            do! Json.write "nick" x.nick
            do! Json.write "isbot" x.isbot
            do! Json.write "status" x.status
            do! Json.write "email" x.email
            do! Json.write "imageUrl" x.imageUrl
        }

    type ChannelInfo = 
        {
            id: ChannelId; name: string; userCount: int; topic: string; joined: bool; users: ChanUserInfo list
        }

        static member FromJson (_: ChannelInfo) = json {
            let! id = Json.read "id"
            let! name = Json.read "name"
            let! userCount = Json.read "userCount"
            let! topic = Json.read "topic"
            let! joined = Json.read "joined"
            let! users = Json.read "users"
            return { id = id; name = name; userCount = userCount; topic = topic; joined = joined; users = users }
        }

        static member ToJson (x: ChannelInfo) = json {
            do! Json.write "id" x.id
            do! Json.write "name" x.name
            do! Json.write "userCount" x.userCount
            do! Json.write "topic" x.topic
            do! Json.write "joined" x.joined
            do! Json.write "users" x.users
        }

    type UserMessageInfo = {text: string; chan: ChannelId}
    type UserCommandInfo = {command: string; chan: ChannelId}

    type ServerCommand =
        | UserCommand of UserCommandInfo
        | Join of ChannelId
        | JoinOrCreate of channelName: string
        | Leave of ChannelId
        | Ping

    type ServerMsg =
        | Greets
        | UserMessage of UserMessageInfo
        | ServerCommand of reqId: string * message: ServerCommand

    type HelloInfo = 
        {
            me: ChanUserInfo
            channels: ChannelInfo list
        }

        static member FromJson (_: HelloInfo) = json {
            let! m = Json.read "me"
            let! c = Json.read "channels"
            return { me = m; channels = c }
        }

        static member ToJson (x: HelloInfo) = json {
            do! Json.write "me" x.me
            do! Json.write "channelse" x.channels
        }

    type ClientErrMsg =
        | AuthFail of string
        | CannotProcess of string

    type ChannelMsgInfo = {
        id: int; ts: System.DateTime; text: string; chan: ChannelId; author: UserId
    }

    type ChannelEventKind =
        | Joined of ChannelId * ChanUserInfo
        | Left of ChannelId * UserId
        | Updated of ChannelId * ChanUserInfo

    type ChannelEventInfo = {
        id: int; ts: System.DateTime
        evt: ChannelEventKind
    }

    type CommandResponse =
        | Error of ClientErrMsg
        | UserUpdated of ChanUserInfo
        | JoinedChannel of ChannelInfo  // client joined a channel
        | LeftChannel of chanId: string
        | Pong

    /// The messages from server to client
    type ClientMsg =
        | Hello of HelloInfo
        | CmdResponse of reqId: string * reply: CommandResponse

        // external events
        | ChanMsg of ChannelMsgInfo
        | ChannelEvent of ChannelEventInfo
        | NewChannel of ChannelInfo
        | RemoveChannel of ChannelInfo

        static member FromJson (_ : ClientMsg) =
            function
            | Property "Hello" h as json -> Json.init (Hello h) json
            | json -> Json.error (sprintf "couldn't deserialise %A to ClientMsg" json) json

        static member ToJson (x: ClientMsg) =
            match x with
            | Hello h -> Json.write "Hello" h
