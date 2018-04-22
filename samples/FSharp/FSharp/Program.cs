using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using BlazorRedux;
using FSharpLib;

namespace FSharp
{
    internal class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddReduxStore<MyState, MyMsg>(options =>
                {
                    options.InitialState = new MyState("", 0, null);
                    options.RootReducer = MyFuncs.MyReducer;
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