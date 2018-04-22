using System.Collections.Generic;

namespace Minimal
{
    public class MyState
    {
        public string Location { get; set; }
        public int Count { get; set; }
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}