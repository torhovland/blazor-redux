using System;

namespace BlazorRedux
{
    public static class UseExtentions
    {
        public static IStoreBuilder<TState, TAction> Use<TState, TAction>(this IStoreBuilder<TState, TAction> builder, Func<TState, TAction, StoreEventDelegate<TState, TAction>, TState> middleware)
        {
            return builder.Use(next =>
            {
                return (state, action) =>
                {
                    Func<TState> simpleNext = () => next(state, action);
                    return middleware(state, action, next);
                };
            });
        }
    }
}
