using System;
using System.Threading.Tasks;

namespace BlazorRedux
{
    public static class UseExtentions
    {
        public static IStoreBuilder<TState, TAction> Use<TState, TAction>(this IStoreBuilder<TState, TAction> builder, Func<TState, TAction, Func<Task>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (state, action) =>
                {
                    Func<Task> simpleNext = () => next(state, action);
                    return middleware(state, action, simpleNext);
                };
            });
        }
    }
}
