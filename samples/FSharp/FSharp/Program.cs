using BlazorRedux;
using FSharpLib;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FSharp
{
    internal class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddSingleton(
                    new Store<MyModel, MyMsg>(
                        MyFuncs.MyReducer, 
                        MyFuncs.LocationReducer, 
                        (state) => state.Location,
                        new MyModel()));
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}