using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using BlazorRedux;
using BlazorFSharpLib;

namespace Blazor
{
    internal class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddReduxStore<MyState, MyMsg>(
                    new MyState("", 0, null), 
                    MyFuncs.MyReducer, 
                    options =>
                {
                    options.LocationReducer = MyFuncs.LocationReducer;
                    options.GetLocation = state => state.Location;
                    options.StateSerializer = MyFuncs.StateSerializer;
                    options.StateDeserializer = MyFuncs.StateDeserializer;
                });
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}