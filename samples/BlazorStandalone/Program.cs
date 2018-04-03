using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using System;
using BlazorRedux;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorStandalone
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddSingleton(new Store<MyModel>(MyModel.Init));
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
