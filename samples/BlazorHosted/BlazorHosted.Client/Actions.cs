using System.Collections.Generic;
using BlazorHosted.Shared;
using BlazorRedux;

namespace BlazorHosted.Client
{
    public class IncrementByOneAction : IAction
    {
    }

    public class IncrementByValueAction : IAction
    {
        public IncrementByValueAction(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }

    public class ClearWeatherAction : IAction
    {
    }

    public class ReceiveWeatherAction : IAction
    {
        public IEnumerable<WeatherForecast> Forecasts { get; set; }
    }
}