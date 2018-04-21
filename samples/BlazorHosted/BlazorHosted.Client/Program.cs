using BlazorRedux;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorHosted.Client
{
    public class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddSingleton(new Store<MyModel, IAction>(
                    Reducers.MainReducer, 
                    Reducers.LocationReducer, 
                    state => state.Location));
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}