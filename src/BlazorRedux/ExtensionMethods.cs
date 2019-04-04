using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddReduxStore<TState, TAction>(
            this IServiceCollection configure,
            TState initialState,
            Reducer<TState, TAction> rootReducer,
            Action<ReduxOptions<TState>> options = null)
        {
            configure.AddSingleton<DevToolsInterop>();
            var reduxOptions = new ReduxOptions<TState>();
            options?.Invoke(reduxOptions);
            configure.AddSingleton<Store<TState, TAction>>(sp => new Store<TState, TAction>(initialState, rootReducer, reduxOptions, sp.GetRequiredService<DevToolsInterop>()));
            return configure;
        }
    }
}