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
                configure.AddSingleton(MyFuncs.InitStore);
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}