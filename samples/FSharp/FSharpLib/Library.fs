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
    inherit ReduxComponent<MyModel>()

module MyFuncs =
    let CountReducer count action =
        match action with
            | IncrementByOne -> count + 1
            | IncrementByValue n -> count + n
            | _ -> count

    let ForecastReducer forecasts action =
        match action with
            | LoadWeather -> None
            | ReceiveWeather r -> Some r
            | _ -> forecasts

    let MainReducer state action =
        printfn "MainReducer"
        {
            Count = CountReducer state.Count action;
            Forecasts = ForecastReducer state.Forecasts action
        }

    let reducer = Reducer<MyModel>(fun state action -> 
        MainReducer state (action :?> MyMsg))

    let InitStore = new Store<_>(reducer, { Count = 0; Forecasts = None })
