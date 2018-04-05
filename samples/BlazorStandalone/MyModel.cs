using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorStandalone
{
    public class MyModel
    {
        [Inject] public HttpClient Http { get; set; }

        public int Count { get; set; }
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}