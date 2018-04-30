using BlazorRedux;

namespace BlazorReduxLocation
{
    public class NewLocationAction : IAction
    {
        public string Location { get; set; }
    }
}
