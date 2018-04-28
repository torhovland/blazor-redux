module Chat.Types

open FsChat

open Channel.Types

type ChatData = {
    ChannelList: Map<ChannelId,ChannelInfo>
    Channels: Map<ChannelId, ChannelData>
    NewChanName: string option   // name for new channel (part of SetCreateChanName), None - panel is hidden
} with
    static member Empty = {
        ChannelList = Map.empty; Channels = Map.empty; NewChanName = None}

type ChatState =
    | NotConnected
    | Connected of UserInfo * ChatData

type Msg =
    | Nop
    | ChannelMsg of ChannelId * Channel.Types.Msg
    | SetNewChanName of string option
    | CreateJoin
    | Join of chanId: string
    | Leave of chanId: string
 