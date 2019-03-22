using BlazorRedux;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Services;
using Microsoft.AspNetCore.Blazor.Hosting;

namespace Minimal
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }
}