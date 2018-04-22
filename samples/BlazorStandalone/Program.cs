using BlazorRedux;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;

namespace BlazorStandalone
{
    internal class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddReduxStore<MyState, IAction>(options =>
                {
                    options.InitialState = new MyState();
                    options.MainReducer = Reducers.MainReducer;
                    options.LocationReducer = Reducers.LocationReducer;
                    options.GetLocation = state => state.Location;
                });
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}