using System.Net.Http;
using BlazorRedux;
using FSharpLib;
using Microsoft.AspNetCore.Blazor;

namespace FSharp
{
    public static class ActionCreators
    {
        public static AsyncActionsCreator<MyModel, MyMsg> LoadWeather(HttpClient http)
        {
            return async (dispatch, state) =>
            {
                dispatch(MyMsg.LoadWeather);

                var forecasts = await http.GetJsonAsync<WeatherForecast[]>(
                    "/sample-data/weather.json");

                dispatch(MyMsg.NewReceiveWeather(forecasts));
            };
        }
    }
}