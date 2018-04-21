using BlazorRedux;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;

namespace BlazorHosted.Client
{
    public class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddReduxStore<MyModel, IAction>(options =>
                {
                    options.InitialState = new MyModel();
                    options.MainReducer = Reducers.MainReducer;
                    options.LocationReducer = Reducers.LocationReducer;
                    options.GetLocation = state => state.Location;
                });
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}