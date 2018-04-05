using System.Collections.Generic;

namespace BlazorStandalone
{
    public class MyModel
    {
        public int Count { get; set; }
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}