using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    public static class ExtensionMethods
    {
        public static void AddReduxStore<TState, TAction>(
            this IServiceCollection configure, 
            Reducer<TState, TAction> rootReducer,
            Action<ReduxOptions<TState>> options)
        {
            var reduxOptions = new ReduxOptions<TState>();
            options(reduxOptions);
            configure.AddSingleton(new Store<TState, TAction>(rootReducer, reduxOptions));
        }
    }
}
