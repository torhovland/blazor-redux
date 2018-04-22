using BlazorRedux;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;

namespace Minimal
{
    internal class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddReduxStore<MyState, IAction>(new MyState(), Reducers.RootReducer);
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}