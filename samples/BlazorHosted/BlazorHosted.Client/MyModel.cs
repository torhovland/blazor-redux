using System.Collections.Generic;
using BlazorHosted.Shared;

namespace BlazorHosted.Client
{
    public class MyModel
    {
        public int Count { get; set; }
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}