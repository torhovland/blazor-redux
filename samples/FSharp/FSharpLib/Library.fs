namespace FSharpLib

open System
open Chiron
open BlazorRedux

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

type MyState =
    { 
        Location: string
        Count: int 
        Forecasts: WeatherForecast list option 
    }

    static member FromJson (_: MyState) = json {
        let! l = Json.read "Location"
        let! c = Json.read "Count"
        let! f = Json.read "Forecasts"
        return { Location = l; Count = c; Forecasts = f }
    }

    static member ToJson (x: MyState) = json {
        do! Json.write "Location" x.Location
        do! Json.write "Count" x.Count
        do! Json.write "Forecasts" x.Forecasts
    }

type MyMsg =
    | IncrementByOne
    | IncrementByValue of n : int
    | ClearWeather
    | ReceiveWeather of r : WeatherForecast list

type MyAppComponent() =
    inherit ReduxComponent<MyState, MyMsg>()

module MyFuncs =
    let MyReducer (state: MyState) action =
        match action with
            | IncrementByOne -> { state with Count = state.Count + 1 }
            | IncrementByValue n -> { state with Count = state.Count + n }
            | ClearWeather -> { state with Forecasts = None }
            | ReceiveWeather r -> { state with Forecasts = Some r }

    let LocationReducer (state: MyState) (action: NewLocationAction) =
        { state with Location = action.Location }

    let StateSerializer (state: MyState) =
        state
        |> Json.serialize
        |> Json.format

    let StateDeserializer str : MyState =
        str
        |> Json.parse
        |> Json.deserialize

module ActionCreators =
    open System.Net.Http
    open FSharp.Control.Tasks

    let LoadWeather (dispatch: Dispatcher<MyMsg>, http: HttpClient) =
        task {
            dispatch.Invoke(MyMsg.ClearWeather) |> ignore

            let! forecastString = http.GetStringAsync("/sample-data/weather.json") |> Async.AwaitTask
            let forecasts: WeatherForecast list =
                forecastString
                |> Json.parse
                |> Json.deserialize
                
            dispatch.Invoke(MyMsg.ReceiveWeather forecasts) |> ignore
        }
