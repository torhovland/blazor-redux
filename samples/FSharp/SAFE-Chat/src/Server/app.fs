module App

open System.Net

open Suave
open Suave.OAuth
open Suave.Authentication
open Suave.Operators
open Suave.Logging
open Suave.Filters
open Suave.Redirection
open Suave.Successful
open Suave.RequestErrors
open Suave.State.CookieStateStore

open Akka.Configuration
open Akka.Actor
open Akkling
open Akkling.Streams

open ChatTypes
open ChatUser
open UserStore
open ChatServer
open Logon
open Suave.Html
open UserSessionFlow

// ---------------------------------
// Web app
// ---------------------------------

module Secrets =

    open System.IO
    open Suave.Utils
    open Microsoft.Extensions.Configuration
    
    let (</>) a b = Path.Combine(a, b)
    let CookieSecretFile = "CHAT_DATA" </> "COOKIE_SECRET"
    let OAuthConfigFile = "CHAT_DATA" </> "suave.oauth.config"

    let readCookieSecret () =
        printfn "Reading configuration data from %s" System.Environment.CurrentDirectory
        if not (File.Exists CookieSecretFile) then
            let secret = Crypto.generateKey Crypto.KeyLength
            do (Path.GetDirectoryName CookieSecretFile) |> Directory.CreateDirectory |> ignore
            File.WriteAllBytes (CookieSecretFile, secret)

        File.ReadAllBytes(CookieSecretFile)

    // Here I'm reading my API keys from file stored in my CHAT_DATA/suave.oauth.config folder
    let private oauthConfigData =
        if not (File.Exists OAuthConfigFile) then
            do (Path.GetDirectoryName OAuthConfigFile) |> Directory.CreateDirectory |> ignore
            File.WriteAllText (OAuthConfigFile, """{
      "google": {
      	"client_id": "<type in client id string>",
      	"client_secret": "<type in client secret>"
      	},
}"""    )

        ConfigurationBuilder().SetBasePath(System.Environment.CurrentDirectory) .AddJsonFile(OAuthConfigFile).Build()

    let dump name a =
        printfn "%s: %A" name a
        a

    let oauthConfigs =
        defineProviderConfigs (fun pname c ->
            let key = pname.ToLowerInvariant()
            {c with
                client_id = oauthConfigData.[key + ":client_id"]
                client_secret = oauthConfigData.[key + ":client_secret"]}
        )
        // |> dump "oauth configs"

type ServerActor = IActorRef<ChatServer.ServerControlMessage>
let mutable private appServerState = None

let startChatServer () = async {
    let config = ConfigurationFactory.ParseString """akka {  
    stdout-loglevel = WARNING
    loglevel = DEBUG
    persistence {
        journal {
            # plugin = "akka.persistence.journal.sqlite"
            sqlite {
                class = "Akka.Persistence.Sqlite.Journal.SqliteJournal, Akka.Persistence.Sqlite"
                plugin-dispatcher = "akka.actor.default-dispatcher"
                connection-string = "Data Source=CHAT_DATA\\journal.db;cache=shared;"
                connection-timeout = 30s
                schema-name = dbo
                table-name = event_journal
                auto-initialize = on
                timestamp-provider = "Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common"
            }
            sql-server {
                class = "Akka.Persistence.SqlServer.Journal.SqlServerJournal, Akka.Persistence.SqlServer"
                connection-string = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=journal;Integrated Security=True;"
                schema-name = dbo
                auto-initialize = on
            }
        }
    }
    actor {
        ask-timeout = 2000
        debug {
            # receive = on
            # autoreceive = on
            # lifecycle = on
            # event-stream = on
            unhandled = on
        }
    }
    }"""
    let actorSystem = ActorSystem.Create("chatapp", config)
    let userStore = UserStore.UserStore actorSystem
    // let _ = SqlitePersistence.Get actorSystem
    do! Async.Sleep(1000)

    let chatServer = ChatServer.startServer actorSystem
    do! Diag.createDiagChannel userStore.GetUser actorSystem chatServer (UserStore.UserIds.echo, "Demo", "Channel for testing purposes. Notice the bots are always ready to keep conversation.")

    // UserStore.

    do! chatServer |> addChannel "Test" "empty channel" None |> Async.Ignore
    do! chatServer |> getOrCreateChannel "About" "interactive help" (AboutFlow.createActor UserStore.UserIds.system) |> Async.Ignore

    appServerState <- Some (actorSystem, userStore, chatServer)
    return ()
}

let logger = Log.create "fschat"

let returnPathOrHome = 
    request (fun x -> 
        match x.queryParam "returnPath" with
        | Choice1Of2 path -> path
        | _ -> "/"
        |> FOUND)

let sessionStore setF = context (fun x ->
    match HttpContext.state x with
    | Some state -> setF state
    | None -> never)

let session (userStore: UserStore) (f: ClientSession -> WebPart) = 
    statefulForSession
    >=> context (HttpContext.state >>
        function
        | None -> f NoSession
        | Some state ->
            match state.get "userid" with
            | Some userid ->
                fun ctx -> async {
                    let! result = userStore.GetUser (UserId userid)
                    match result with
                    | Some me ->
                        return! f (UserLoggedOn me) ctx
                    | None ->
                        logger.error (Message.eventX "Failed to get user from user store {id}" >> Message.setField "id" userid)
                        return! f NoSession ctx
                }
                
            | _ -> f NoSession)

let noCache =
    Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
    >=> Writers.setHeader "Pragma" "no-cache"
    >=> Writers.setHeader "Expires" "0"

let getPayloadString req = System.Text.Encoding.UTF8.GetString(req.rawForm)

let getUserImageUrl (claims: Map<string,obj>) : string option =
    let getClaim claim () = claims |> Map.tryFind claim |> Option.map string

    None
    |> Option.orElseWith (getClaim "avatar_url")
    |> Option.orElseWith (getClaim "picture")

let root: WebPart =
  warbler(fun _ ->
    match appServerState with
    | Some (actorSystem, userStore, server) ->
        choose [
            warbler(fun ctx ->
                // problem is that redirection leads to localhost and authorization does not go well
                let authorizeRedirectUri =
                    (ctx.runtime.matchedBinding.uri "oalogin" "").ToString().Replace("127.0.0.1", "localhost")

                authorize authorizeRedirectUri Secrets.oauthConfigs
                    (fun loginData ctx -> async {
                        let imageUrl =
                            getUserImageUrl loginData.ProviderData
                            |> Option.orElseWith (fun () -> makeUserImageUrl "wavatar" loginData.Name)

                        let user = Person {
                            oauthId = Some loginData.Id
                            nick = loginData.Name; status = ""; email = None; imageUrl = imageUrl}

                        let! registerResult = userStore.Register user
                        match registerResult with
                        | Ok (UserId userid) ->
                            
                            logger.info (Message.eventX "User registered via oauth \"{name}\""
                                >> Message.setFieldValue "name" loginData.Name)

                            return! (statefulForSession
                                >=> sessionStore (fun store -> store.set "userid" userid)
                                >=> FOUND "/") ctx
                        | Result.Error message ->
                            return! (OK <| sprintf "Register failed because of `%s`" message) ctx
                        }
                    )
                    (fun () -> FOUND "/logon")
                    (fun error -> OK <| sprintf "Authorization failed because of `%s`" error.Message)
                )

            session userStore (fun session ->
                choose [
                    GET >=> path "/" >=> noCache >=> (
                        match session with
                        | NoSession -> found "/logon"
                        | _ -> Files.browseFileHome "index.html"
                        )
                    // handlers for login form
                    path "/logon" >=> choose [
                        GET >=> noCache >=>
                            (Logon.Views.index session |> htmlToString |> OK)
                        POST >=> (
                            fun ctx -> async {
                                let nick = (getPayloadString ctx.request).Substring 5
                                           |> WebUtility.UrlDecode  |> WebUtility.HtmlDecode
                                let imageUrl = makeUserImageUrl "monsterid" nick
                                let user = Anonymous {nick = nick; status = ""; imageUrl = imageUrl}
                                let! registerResult = userStore.Register user
                                match registerResult with
                                | Ok (UserId userid) ->
                                    logger.info (Message.eventX "Anonymous login by nick {nick}"
                                        >> Message.setFieldValue "nick" nick)

                                    return! (statefulForSession
                                        >=> sessionStore (fun store -> store.set "userid" userid)
                                        >=> FOUND "/") ctx
                                | Result.Error message ->
                                    return! (OK <| sprintf "Register failed because of `%s`" message) ctx
                            }
                        )
                    ]
                    GET >=> path "/logoff" >=> noCache >=>
                        deauthenticate >=> (warbler(fun _ ->
                            match session with
                            | UserLoggedOn (RegisteredUser (userid, Anonymous { nick = nick})) ->
                                logger.info (Message.eventX "LOGOFF: Unregistering anonymous {nick}"
                                    >> Message.setFieldValue "nick" nick)
                                do userStore.Unregister userid
                            | _ -> ()
                            FOUND "/logon"
                        ))

                    path "/api/socket" >=>
                        match session with
                        | UserLoggedOn user -> fun ctx -> async {
                            let session = UserSession.Session(server, userStore, user)
                            let materializer = actorSystem.Materializer()

                            let messageFlow = createMessageFlow materializer
                            let socketFlow = createSessionFlow userStore messageFlow session.ControlFlow

                            let materialize materializer source sink =
                                session.SetListenChannel(
                                    source
                                    |> Source.viaMat socketFlow Keep.right
                                    |> Source.toMat sink Keep.left
                                    |> Graph.run materializer |> Some)
                                ()

                            logger.debug (Message.eventX "Opening socket for {user}" >> Message.setField "user" (getUserNick user))
                            let! result = WebSocket.handShake (SocketFlow.handleWebsocketMessages actorSystem materialize) ctx
                            logger.debug (Message.eventX "Closing socket for {user}" >> Message.setField "user" (getUserNick user))

                            return result
                            }
                        | NoSession ->
                            BAD_REQUEST "Authorization required"

                    Files.browseHome
                    ]
            )

            NOT_FOUND "Not Found"
        ]
    | None -> ServerErrors.SERVICE_UNAVAILABLE "Server is not started"
  )