namespace BlazorRedux
{
    public static class Location
    {
        public static string Reducer(string location, IAction action)
        {
            switch (action)
            {
                case NewLocationAction a:
                    return a.Location;
                default:
                    return location;
            }
        }
    }
}
