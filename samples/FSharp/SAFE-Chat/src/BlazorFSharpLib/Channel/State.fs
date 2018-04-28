module Channel.State

open Types

let getUserNick userid users =
    users |> Map.tryFind userid |> Option.map (fun user -> user.Nick)

let unknownUser userId () = {
    Id = userId; Nick = "Unknown #" + userId; Status = ""
    IsBot = false; Online = true; ImageUrl = None; isMe = false}
