using System;
using System.Threading.Tasks;

namespace BlazorRedux
{
    public static class UseExtentions
    {
        public static IStoreBuilder<TState, TAction> Use<TState, TAction>(this IStoreBuilder<TState, TAction> builder, Func<TState, TAction, StoreEventDelegate<TState, TAction>, Task> middleware)
        {
            return builder.Use(next =>
            {
                return (state, action) =>
                {
                    //Func<Task> simpleNext = () =>
                    //{
                    //    var _state = state;
                    //    return next(ref _state, action);
                    //};
                    return middleware(state, action, next);
                };
            });
        }
    }
}
