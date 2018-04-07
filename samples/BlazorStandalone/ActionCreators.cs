using System.Net.Http;
using BlazorRedux;
using Microsoft.AspNetCore.Blazor;

namespace BlazorStandalone
{
    public static class ActionCreators
    {
        public static AsyncActionsCreator<MyModel, IAction> LoadWeather(HttpClient http)
        {
            return async (dispatch, state) =>
            {
                dispatch(new ClearWeatherAction());

                var forecasts = await http.GetJsonAsync<WeatherForecast[]>(
                    "/sample-data/weather.json");

                dispatch(new ReceiveWeatherAction
                {
                    Forecasts = forecasts
                });
            };
        }
    }
}