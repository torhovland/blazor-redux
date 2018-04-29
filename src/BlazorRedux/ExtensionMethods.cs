using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    public static class ExtensionMethods
    {
        public static Store<TState, TAction> AddReduxStore<TState, TAction>(
            this IServiceCollection configure,
            TState initialState,
            Reducer<TState, TAction> rootReducer,
            Action<ReduxOptions<TState>> options = null)
        {
            var reduxOptions = new ReduxOptions<TState>();
            options?.Invoke(reduxOptions);
            var store = new Store<TState, TAction>(initialState, rootReducer, reduxOptions);
            configure.AddSingleton(store);
            return store;
        }
    }
}