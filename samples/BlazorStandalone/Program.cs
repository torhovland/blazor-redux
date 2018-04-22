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
                configure.AddReduxStore<MyState, IAction>(Reducers.RootReducer, options =>
                {
                    options.InitialState = new MyState();
                    options.LocationReducer = Reducers.LocationReducer;
                    options.GetLocation = state => state.Location;
                });
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}