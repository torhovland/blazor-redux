open System.IO

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Logging

type CmdArgs = { IP: System.Net.IPAddress; Port: Sockets.Port; ClientPath: string }

module TweakingSuave =

    open Newtonsoft.Json

    let utf8 = System.Text.Encoding.UTF8

    type JsonNetCookieSerialiser () =
      interface CookieSerialiser with
        member x.serialise m =
          utf8.GetBytes (JsonConvert.SerializeObject m)
        member x.deserialise m =
          JsonConvert.DeserializeObject<Map<string, obj>> (utf8.GetString m)

let (</>) a b = Path.Combine(a, b)

[<EntryPoint>]
let main argv =

    let args =
        let parse f str = match f str with (true, i) -> Some i | _ -> None

        let (|Port|_|) = parse System.UInt16.TryParse
        let (|IPAddress|_|) = parse System.Net.IPAddress.TryParse

        //default bind to 127.0.0.1:8083
        let defaultArgs = {
            IP = System.Net.IPAddress.Loopback; Port = 8083us
            ClientPath = ".." </> "Client" </> "public"
            }

        let rec parseArgs b args =
            match args with
            | [] -> b
            | "--ip" :: IPAddress ip :: xs -> parseArgs { b with IP = ip } xs
            | "--port" :: Port p :: xs -> parseArgs { b with Port = p } xs
            | "-cp" :: clientPath :: xs
            | "--clientpath" :: clientPath :: xs -> parseArgs { b with ClientPath = clientPath } xs
            | invalidArgs ->
                printfn "error: invalid arguments %A" invalidArgs
                printfn "Usage:"
                printfn "    --ip ADDRESS      ip address (Default: %O)" defaultArgs.IP
                printfn "    --port PORT       port (Default: %i)" defaultArgs.Port
                printfn "    --clientpath PATH client bundle path(Default: %s)" defaultArgs.ClientPath
                exit 1

        argv |> List.ofArray |> parseArgs defaultArgs

    let logger = Logging.Targets.create Logging.Verbose [| "Suave" |]

    let app = App.root >=> logWithLevelStructured Logging.Info logger logFormatStructured

    let config =
        { defaultConfig with
            logger = Targets.create LogLevel.Debug [|"ServerCode"; "Server" |]
            bindings = [ HttpBinding.create HTTP args.IP args.Port ]
            cookieSerialiser = TweakingSuave.JsonNetCookieSerialiser()
            homeFolder = args.ClientPath |> (Path.GetFullPath >> Some)

            serverKey = App.Secrets.readCookieSecret()
        }

    let cts = new System.Threading.CancellationTokenSource()
    let application = async {
        let _, webServer = startWebServerAsync config app
        do! App.startChatServer()
        do! webServer
     
        return ()
    }

    Async.Start (application, cts.Token)

    //kill the server
    printfn "type 'q' to gracefully stop"
    while "q" <> System.Console.ReadLine() do ()
    cts.Cancel()

    0 // return an integer exit code
