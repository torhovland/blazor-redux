namespace FSharpLib

open System
open BlazorRedux

type WeatherForecast() =
    member val Date = DateTime.MinValue with get, set
    member val TemperatureC = 0 with get, set
    member val TemperatureF = 0 with get, set
    member val Summary = "" with get, set

type MyModel =
    {
        Count: int;
        Forecasts: WeatherForecast[] option;
    }

type MyMsg =
    | IncrementByOne
    | IncrementByValue of n : int
    | LoadWeather
    | ReceiveWeather of r : WeatherForecast[]

type MyAppComponent() =
    inherit ReduxComponent<MyModel, MyMsg>()

module MyFuncs =
    let MyReducer state action =
        match action with
            | IncrementByOne -> { state with Count = state.Count + 1 }
            | IncrementByValue n -> { state with Count = state.Count + n }
            | LoadWeather -> { state with Forecasts = None }
            | ReceiveWeather r -> { state with Forecasts = Some r }

    let InitStore = new Store<MyModel, MyMsg>(Reducer<MyModel, MyMsg>MyReducer, { Count = 0; Forecasts = None })
