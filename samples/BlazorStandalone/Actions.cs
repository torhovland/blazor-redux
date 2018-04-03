namespace BlazorStandalone
{
    public class IncrementAction
    {
        public IncrementAction(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
    }

    public class LoadWeatherAction {}
}
