using System.Collections.Generic;

namespace BlazorStandalone
{
    public class MyModel
    {
        public string Location { get; set; }
        public int Count { get; set; }
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}