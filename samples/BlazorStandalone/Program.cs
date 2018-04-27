using BlazorRedux;
using BlazorReduxLogger;
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
                    Console.WriteLine("Inline logger: {0}", action);
                    return next();
                });

                builder.UseMiddleware<Logger<MyState, IAction>, MyState, IAction>(serviceProvider);

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