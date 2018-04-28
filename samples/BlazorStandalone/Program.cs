using BlazorRedux;
using BlazorReduxLogger;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;
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
                    Console.WriteLine("Inline logger: {0}", JsonUtil.Serialize(action));
                    return next();
                });

                Func<object, string> fn = obj => JsonUtil.Serialize(obj);
                builder.UseMiddleware<Logger<MyState, IAction>, MyState, IAction>(serviceProvider, fn);

                builder.Use((state, action, next) =>
                {
                    Console.WriteLine("2nd Inline logger after middleware class: {0}", action);
                    return next();
                });
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}