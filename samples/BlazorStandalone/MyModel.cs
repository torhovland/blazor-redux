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
            if (action is IncrementAction i)
            {
                Count += i.Value;
            }
            else if (action is LoadWeatherAction)
            {
                Forecasts = await Http.GetJsonAsync<WeatherForecast[]>(
                    "/sample-data/weather.json");
            }
        }
    }
}
