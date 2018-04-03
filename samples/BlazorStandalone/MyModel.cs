using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorRedux;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorStandalone
{
    public class MyModel : IModel
    {
        [Inject]
        public HttpClient Http { get; set; }

        public int Count { get; private set; }
        public IEnumerable<WeatherForecast> Forecasts { get; private set; }

        public static MyModel Init()
        {
            return new MyModel
            {
                Count = 5,
                Forecasts = new List<WeatherForecast>()
            };
        }

        public async Task ProcessAsync(object action)
        {
            switch (action)
            {
                case IncrementByValueAction a:
                    Count += a.Value;
                    break;
                case LoadWeatherAction a:
                    Forecasts = await a.Http.GetJsonAsync<WeatherForecast[]>(
                        "/sample-data/weather.json");
                    break;
            }
        }
    }
}
