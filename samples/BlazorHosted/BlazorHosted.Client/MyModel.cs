using System.Collections.Generic;
using System.Linq;
using BlazorHosted.Shared;

namespace BlazorHosted.Client
{
    public class MyModel
    {
        public string Location { get; set; }
        public int Count { get; set; }
        public IEnumerable<WeatherForecast> Forecasts { get; set; }

        public override string ToString()
        {
            var forecasts = string.Join("\n", 
                (Forecasts ?? new WeatherForecast[0]).Select(f => f.ToString()));

            return $"Count: {Count}\n\nForecasts:\n\n{forecasts}";
        }
    }
}