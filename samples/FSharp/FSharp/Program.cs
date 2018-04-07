using BlazorRedux;
using FSharpLib;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FSharp
{
    internal class Program
    {
        private static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddSingleton(
                    new Store<MyModel, MyMsg>(
                        MyFuncs.MyReducer, 
                        new MyModel(0, null)));
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}