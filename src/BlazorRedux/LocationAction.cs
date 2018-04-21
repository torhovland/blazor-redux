namespace BlazorRedux
{
    public class LocationAction
    {
    }

    public class NewLocationAction : LocationAction
    {
        public string Location { get; set; }
    }
}
