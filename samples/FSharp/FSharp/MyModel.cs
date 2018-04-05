using System.Collections.Generic;

namespace FSharp
{
    public class MyModel
    {
        public int Count { get; set; }
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}