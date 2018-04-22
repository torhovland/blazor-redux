module Chat.Types

open FsChat
open Fable.Websockets.Elmish
open Fable.Websockets.Elmish.Types

open Channel.Types

type ChatData = {
    socket: SocketHandle<Protocol.ServerMsg, Protocol.ClientMsg>
    ChannelList: Map<ChannelId,ChannelInfo>
    Channels: Map<ChannelId, ChannelData>
    NewChanName: string option   // name for new channel (part of SetCreateChanName), None - panel is hidden
} with
    static member Empty = {
        socket = SocketHandle.Blackhole()
        ChannelList = Map.empty; Channels = Map.empty; NewChanName = None}

type ChatState =
    | NotConnected
    | Connected of UserInfo * ChatData

type AppMsg =
    | Nop
    | ChannelMsg of ChannelId * Channel.Types.Msg
    | SetNewChanName of string option
    | CreateJoin
    | Join of chanId: string

    | Leave of chanId: string
 
type Msg = Msg<Protocol.ServerMsg, Protocol.ClientMsg, AppMsg>
