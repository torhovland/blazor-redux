using BlazorRedux;
using BlazorReduxLogger;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using System;

namespace BlazorStandalone
{
    internal class Program
    {
        public static void Main()
        {
            Store<MyState, IAction> store = null;
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                store = configure.AddReduxStore<MyState, IAction>(
                    new MyState(), Reducers.RootReducer, options =>
                {
                    options.GetLocation = state => state.Location;
                });
            });

            store.ApplyMiddleware(builder =>
            {
                builder.Use((state, action, next) =>
                {
                    Console.WriteLine("Inline logger old state: {0}", JsonUtil.Serialize(state));
                    Console.WriteLine("Inline logger action: {0}", JsonUtil.Serialize(action));
                    var newState = next(state, action);
                    Console.WriteLine("Inline logger new state: {0}", JsonUtil.Serialize(newState));
                    return newState;
                });

                Func<object, string> fn = obj => JsonUtil.Serialize(obj);
                builder.UseMiddleware<Logger<MyState, IAction>, MyState, IAction>(serviceProvider, fn);
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}