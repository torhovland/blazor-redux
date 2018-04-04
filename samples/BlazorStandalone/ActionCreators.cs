using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorRedux;
using Microsoft.AspNetCore.Blazor;

namespace BlazorStandalone
{
    public static class ActionCreators
    {
        public static AsyncActionsCreator<MyModel> LoadWeather(HttpClient http)
        {
            return async (dispatch, state) =>
            {
                dispatch(new LoadWeatherAction());

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
