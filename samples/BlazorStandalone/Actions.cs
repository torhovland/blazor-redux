using System.Collections.Generic;

namespace BlazorStandalone
{
    public class IncrementByOneAction
    {
    }

    public class IncrementByValueAction
    {
        public IncrementByValueAction(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }

    public class LoadWeatherAction
    {
    }

    public class ReceiveWeatherAction
    {
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}