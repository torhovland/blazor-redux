using System;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    public static class ExtensionMethods
    {
        public static void AddReduxStore<TState, TAction>(this IServiceCollection configure, Action<ReduxOptions<TState, TAction>> options)
        {
            var reduxOptions = new ReduxOptions<TState, TAction>();
            options(reduxOptions);
            configure.AddSingleton(new Store<TState, TAction>(reduxOptions));
        }
    }
}
