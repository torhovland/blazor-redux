module Channel.Types

type UserId = string
type UserInfo = {
    Id: UserId
    Nick: string; Status: string
    IsBot: bool; Online: bool; ImageUrl: string option; isMe: bool}
with static member Anon = {Id = "0"; Nick = "anonymous"; Status = ""; IsBot = false; Online = true; ImageUrl = None; isMe = false}

type Message =
    | UserMessage of text: string * author: UserInfo
    | SystemMessage of text: string

type 't Envelope = {
    Id: int
    Ts: System.DateTime
    Content: 't
}

type ChannelId = string // I do know it should be single case type
type ChannelInfo = {
    Id: string
    Name: string
    Topic: string
    UserCount: int
} with static member Empty = {Id = null; Name = null; Topic = ""; UserCount = 0}

type ChannelData = {
    Info: ChannelInfo
    Users: Map<UserId, UserInfo>
    Messages: Message Envelope list
    PostText: string
}

type Msg =
    | Init of ChannelInfo * UserInfo list
    | Update of ChannelInfo
    | AppendMessage of Message Envelope
    | AppendUserMessage of UserId * string Envelope

    | UserJoined of UserInfo
    | UserLeft of UserId
    | UserUpdated of UserInfo

    | SetPostText of string
    | PostText
    // the following are messages for container
    | Forward of string
    | Leave