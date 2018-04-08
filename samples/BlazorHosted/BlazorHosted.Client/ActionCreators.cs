using System.Net.Http;
using BlazorHosted.Shared;
using BlazorRedux;
using Microsoft.AspNetCore.Blazor;

namespace BlazorHosted.Client
{
    public static class ActionCreators
    {
        public static AsyncActionsCreator<MyModel, IAction> LoadWeather(HttpClient http)
        {
            return async (dispatch, state) =>
            {
                dispatch(new ClearWeatherAction());

                var forecasts = await http.GetJsonAsync<WeatherForecast[]>(
                    "/api/SampleData/WeatherForecasts");

                dispatch(new ReceiveWeatherAction
                {
                    Forecasts = forecasts
                });
            };
        }
    }
}