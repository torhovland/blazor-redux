using System.Net.Http;
using System.Threading.Tasks;
using BlazorHosted.Shared;
using BlazorRedux;
using Microsoft.AspNetCore.Components;

namespace BlazorHosted.Client
{
    public static class ActionCreators
    {
        public static async Task LoadWeather(Dispatcher<IAction> dispatch, HttpClient http)
        {
            dispatch(new ClearWeatherAction());

            var forecasts = await http.GetJsonAsync<WeatherForecast[]>(
                "/api/SampleData/WeatherForecasts");

            dispatch(new ReceiveWeatherAction
            {
                Forecasts = forecasts
            });
        }
    }
}