namespace BlazorFSharpLib

open System
open Chiron
open BlazorRedux
open App.Types

// Currently need to help the JSON serialization required by Redux DevTools
// using Chiron. It would be nice to have this totally automatic, but 
// SimpleJson doesn't understand F# record types, and Mono on WebAssembly 
// doesn't currently support Reflection.Emit, which Json.NET and some other
// libraries depend on.
type WeatherForecast =
    { 
        Date: DateTimeOffset
        TemperatureC: int 
        TemperatureF: int 
        Summary: string 
    }

    static member FromJson (_: WeatherForecast) = json {
        let! d = Json.read "Date"
        let! c = Json.read "TemperatureC"
        let! f = Json.read "TemperatureF"
        let! s = Json.read "Summary"
        return { Date = d; TemperatureC = c; TemperatureF = f; Summary = s }
    }

    static member ToJson (x: WeatherForecast) = json {
        do! Json.write "Date" x.Date
        do! Json.write "TemperatureC" x.TemperatureC
        do! Json.write "TemperatureF" x.TemperatureF
        do! Json.write "Summary" x.Summary
    }

type ChatAppComponent() =
    inherit ReduxComponent<Model, Chat.Types.Msg>()

type SocketInterop() =
    static member MessageReceived str = 
        printfn "Received message: %s" str
        let m = 
            str
            |> Json.parse
            |> Json.deserialize
        printfn "%s" m

module MyFuncs =
    let MyReducer state (action: Chat.Types.Msg) =
        match action with
            | Chat.Types.Msg.CreateJoin -> state
            | _ -> state

    let LocationReducer state (action: NewLocationAction) =
        { state with currentPage = action.Location }

    let StateSerializer (state: Model) =
        state
        |> Json.serialize
        |> Json.format

    let StateDeserializer str : Model =
        str
        |> Json.parse
        |> Json.deserialize
