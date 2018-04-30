using BlazorRedux;
using BlazorReduxLocation;
using BlazorReduxLogger;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.AspNetCore.Blazor.Services;

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
                /*builder.Use((state, action, next) =>
                {
                    //This is how you define inline middleware this one does nothing
                    //Do stuff here to change state or action prior to hitting the store
                    var newState = next(state, action);
                    //Do stuff here with state or action after they hit the store
                    return newState;
                });*/

                //builder.UseMiddleware<Logger<MyState, IAction>, MyState, IAction>(serviceProvider);
                builder.UseMiddleware<Location<MyState, IAction>, MyState, IAction>(serviceProvider);
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}