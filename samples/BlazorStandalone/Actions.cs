using System.Net.Http;

namespace BlazorStandalone
{
    public class IncrementByOneAction {}

    public class IncrementByValueAction
    {
        public IncrementByValueAction(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
    }

    public class LoadWeatherAction
    {
        public LoadWeatherAction(HttpClient http)
        {
            Http = http;
        }

        public HttpClient Http { get; private set; }
    }
}
