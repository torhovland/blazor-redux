module ChatUser

open ChatTypes

type PersonalInfo = {oauthId: string option; nick: string; status: string; email: string option; imageUrl: string option}
type AnonymousUserInfo = {nick: string; status: string; imageUrl: string option}

type UserKind =
| Person of PersonalInfo
| Bot of PersonalInfo
| Anonymous of AnonymousUserInfo
| System

type RegisteredUser = RegisteredUser of UserId * UserKind

let empty = {nick = ""; status = ""; email = None; imageUrl = None; oauthId = None}

let makeUserImageUrl deflt = // FIXME find the place for the method
    let computeMd5 (text: string) =
        use md5 = System.Security.Cryptography.MD5.Create()
        let hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))
        System.BitConverter.ToString(hash).Replace("-", "").ToLower()

    function
    | null | "" -> None
    | name -> name |> (computeMd5 >> sprintf "https://www.gravatar.com/avatar/%s?d=%s" >< deflt >> Some)

let getUserNick (RegisteredUser (_, user)) =
    match user with
    | Anonymous {nick = nick}
    | Bot {nick = nick}
    | Person {nick = nick} -> nick
    | System -> "system"

type GetUser = UserId -> RegisteredUser option Async
