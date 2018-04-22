using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    public static class ExtensionMethods
    {
        public static void AddReduxStore<TState, TAction>(
            this IServiceCollection configure,
            TState initialState,
            Reducer<TState, TAction> rootReducer,
            Action<ReduxOptions<TState>> options = null)
        {
            var reduxOptions = new ReduxOptions<TState>();
            options?.Invoke(reduxOptions);
            configure.AddSingleton(new Store<TState, TAction>(initialState, rootReducer, reduxOptions));
        }
    }
}