namespace FSharpLib

open System
open BlazorRedux
open System.Collections.Generic

type WeatherForecast() =
    member val Date = DateTime.MinValue with get, set
    member val TemperatureC = 0 with get, set
    member val TemperatureF = 0 with get, set
    member val Summary = "" with get, set

// It would be nice to be able to use F# record types here, but:
// JsonUtil doesn't understand F# record types, and Mono on WebAssembly 
// doesn't currently support Reflection.Emit, which denies us the use of
// automatic JSON deserialization tools like Json.NET. We need the JSON
// deserialization in order to receive state from Redux DevTools.
type MyModel() =
    member val Location = "" with get, set
    member val Count = 0 with get, set
    member val Forecasts: IEnumerable<WeatherForecast> = [] :> seq<WeatherForecast> with get, set
    member this.Clone() = this.MemberwiseClone() :?> MyModel

type MyMsg =
    | IncrementByOne
    | IncrementByValue of n : int
    | ClearWeather
    | ReceiveWeather of r : WeatherForecast[]

type MyAppComponent() =
    inherit ReduxComponent<MyModel, MyMsg>()

module MyFuncs =
    // These would have been nicer if it was possible to define MyModel as a record type.
    let MyReducer (state: MyModel) action =
        match action with
            | IncrementByOne -> 
                let newState = state.Clone()
                newState.Count <- newState.Count + 1
                newState
            | IncrementByValue n ->
                let newState = state.Clone()
                newState.Count <- newState.Count + n
                newState
            | ClearWeather ->
                let newState = state.Clone()
                newState.Forecasts <- []
                newState
            | ReceiveWeather r ->
                let newState = state.Clone()
                newState.Forecasts <- r
                newState

    let LocationReducer (state: MyModel) (action: LocationAction) =
        match action with
            | :? NewLocationAction as a ->
                let newState = state.Clone()
                newState.Location <- a.Location
                newState
            | _ -> state

module ActionCreators =
    open System.Net.Http
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Blazor

    let LoadWeather (dispatch: Dispatcher<MyMsg>, http: HttpClient) =
        task {
            dispatch.Invoke(MyMsg.ClearWeather) |> ignore
            let! forecasts = http.GetJsonAsync<WeatherForecast[]>("/sample-data/weather.json") |> Async.AwaitTask
            dispatch.Invoke(MyMsg.ReceiveWeather forecasts) |> ignore
        }
