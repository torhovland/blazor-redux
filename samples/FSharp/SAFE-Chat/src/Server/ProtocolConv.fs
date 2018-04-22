module ProtocolConv

open System
open ChatTypes
open ChatUser
open ChatServer

open FsChat

let (|IsChannelId|_|) s = 
    let (result, value) = Int32.TryParse s
    if result then Some (ChannelId value) else None

let makeBlankUserInfo userid nick :Protocol.ChanUserInfo =
    {id = userid; nick = nick; isbot = false; status = ""; email = null; imageUrl = null}
let mapUserToProtocol (RegisteredUser (UserId userid, user)) :Protocol.ChanUserInfo =
    let tostr = Option.toObj

    match user with
    | Person u ->
        {id = userid; nick = u.nick; isbot = false; status = u.status; email = tostr u.email; imageUrl = Option.toObj u.imageUrl}
    | Bot u ->
        {id = userid; nick = u.nick; isbot = true; status = u.status; email = tostr u.email; imageUrl = tostr u.imageUrl}
    | Anonymous info ->
        {makeBlankUserInfo userid info.nick with isbot = false; status = info.status; imageUrl = tostr info.imageUrl}
    | System ->
        {makeBlankUserInfo userid "system" with imageUrl = "/system.png" }

let mapChanInfo ({name = name; topic = topic; id = (ChannelId id)}: ChannelData) : Protocol.ChannelInfo =
    {id = id.ToString(); name = name; topic = topic; userCount = 0; users = []; joined = false}
