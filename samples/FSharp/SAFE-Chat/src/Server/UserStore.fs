module UserStore
// implements users catalog + persistance

open Akkling
open Akkling.Persistence

open ChatTypes
open ChatUser

module UserIds =

    let system = UserId "sys"
    let echo = UserId "echo"

module public Implementation =

    type ErrorInfo = ErrorInfo of string

    type StoreCommand =
        | Register of UserKind
        | Unregister of UserId
        | Update of RegisteredUser
        | GetUsers of UserId list

    type StoreEvent =
        | AddUser of RegisteredUser
        | DropUser of UserId
        | UpdateUser of RegisteredUser

    type ReplyMessage =
        | RegisterResult of Result<UserId, ErrorInfo>
        | UpdateResult of Result<RegisteredUser, ErrorInfo>
        | GetUsersResult of RegisteredUser list

    type StoreMessage =
        | Event of StoreEvent
        | Command of StoreCommand

    type State = {
        nextId: int
        users: Map<UserId, RegisteredUser>
    }

    let createUser userid user = Map.add userid (RegisteredUser(userid, user))
    let makeBot nick = Bot {ChatUser.empty with nick = nick; imageUrl = makeUserImageUrl "robohash" "echobott"}

    let initialState = {
        nextId = 100
        users = Map.empty
            |> createUser UserIds.system System
            |> createUser UserIds.echo (makeBot "echo")
    }

    let lookupNick nickName _ user =
        getUserNick user = nickName

    let updateUserKind =
        function
        | Anonymous _, Anonymous n -> Ok <| Anonymous n
        | Person p, Person n -> Ok <| Person {n with oauthId = p.oauthId}
        | Bot p, Bot n -> Ok <| Bot {n with oauthId = p.oauthId} // id cannot be overwritten
        | _ -> Result.Error <| ErrorInfo "Cannot update user because of different type"

    let updateUser (RegisteredUser (userid, newuser) as uxx) (users: Map<UserId, RegisteredUser>) : Result<_,ErrorInfo> =
        let newNick = getUserNick uxx
        match users |> Map.tryFindKey (lookupNick newNick) with
        | Some foundUserId when foundUserId <> userid ->
            Result.Error <| ErrorInfo "Updated nick was already taken by other user"
        | _ ->
            match users |> Map.tryFind userid with
            | Some (RegisteredUser (_, user)) -> updateUserKind (user, newuser)
            | _ -> Result.Error <| ErrorInfo "User not found, nothing to update"
            |> Result.map(fun u ->
                let newUser = RegisteredUser (userid, u)
                newUser )

    // in case user is logging anonymously check he cannot squote someone's nick
    let (|AnonymousBusyNick|_|) (users: Map<UserId, RegisteredUser>) =
        function
        | Anonymous {nick = userNick} ->
            users |> Map.tryFindKey (lookupNick userNick) |> Option.map(fun uid -> uid, userNick)
        | _ -> None

    let (|AlreadyRegisteredOAuth|_|) (users: Map<UserId, RegisteredUser>) =
        let lookup oauthIdArg _ = function
            | (RegisteredUser (_, Person {oauthId = Some probe})) -> probe = oauthIdArg
            | _ -> false

        function
        | Person {oauthId = Some oauthId} ->
            users |> Map.tryFindKey (lookup oauthId)
        | _ -> None

    let update (state: State) = function
        | AddUser user ->
            let (RegisteredUser (userId, _)) = user
            // FIXME nextId should be compared to userId
            {state with nextId = state.nextId + 1; users = state.users |> Map.add userId user}
        | DropUser userid ->
            {state with users = state.users |> Map.remove userid}
        | UpdateUser user ->
            let (RegisteredUser (userId, _)) = user
            {state with users = state.users |> Map.add userId user}

    let handler (ctx: Eventsourced<_>) =
        let rec loop (state: State) = actor {
            let! msg = ctx.Receive()
            let reply = (<!) (ctx.Sender())

            match msg with
            | Event e ->
                return! loop (update state e)
            | Command cmd ->
                match cmd with
                | Register (user) ->
                    match user with
                    | AnonymousBusyNick state.users (UserId uid, nickname) ->
                        let errorMessage = sprintf "The nickname %s is already taken by user %s" nickname uid
                        errorMessage |> (ErrorInfo >> Result.Error >> RegisterResult >> reply)
                        return loop state
                    | AlreadyRegisteredOAuth state.users userId ->
                        userId |> (Ok >> RegisterResult >> reply)
                        return loop state
                    | _ ->
                        let userId = UserId <| state.nextId.ToString()
                        userId |> (Ok >> RegisterResult >> reply)
                        return Persist (Event <| AddUser (RegisteredUser (userId, user)))

                | Unregister userid ->
                    return Persist (Event <| DropUser userid)

                | Update user ->
                    match state.users |> updateUser user with
                    | Ok(newUser) ->
                        newUser |> (Ok >> UpdateResult >> reply)
                        return Persist (Event <| UpdateUser newUser)
                    | Result.Error e ->
                        e |> (Result.Error >> UpdateResult >> reply)
                        return loop state

                | GetUsers (userids) ->
                    userids |> List.collect (Map.tryFind >< state.users >> Option.toList) |> (GetUsersResult >> reply)
                    return loop state
        }
        loop initialState

open Implementation

type UserStore(system: Akka.Actor.ActorSystem) =

    let storeActor = spawn system "userstore" <| propsPersist(handler)

    member _this.Register(user: UserKind) : Result<UserId,string> Async =
        async {
            let! (RegisterResult result | OtherwiseFail result) = storeActor <? (Command <| Register user)
            return result |> Result.mapError (fun (ErrorInfo error) -> error)
        }

    member _this.Unregister (userid: UserId) : unit =
        storeActor <! (Command <| Unregister userid)

    member _this.Update(user: RegisteredUser) : Result<RegisteredUser,string> Async =
        async {
            let! (UpdateResult result | OtherwiseFail result) = storeActor <? (Command <| Update user)
            return result |> Result.mapError (fun (ErrorInfo error) -> error)
        }

    member _this.GetUser userid : RegisteredUser option Async =
        async {
            let! (GetUsersResult result | OtherwiseFail result) = storeActor <? (Command <| GetUsers [userid])
            return result |> function | [] -> None | x::_ -> Some x
        }

    member _this.GetUsers (userids: UserId list) : RegisteredUser list Async =
        async {
            let! (GetUsersResult result | OtherwiseFailErr "no choice" result) = storeActor <? (Command <| GetUsers userids)
            return result
        }
