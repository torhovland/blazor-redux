using BlazorRedux;
using System;

namespace BlazorReduxLogger
{
    public class Logger<TState, TAction>
    {
        private readonly StoreEventDelegate<TState, TAction> _next;
        private readonly Func<object, string> _serialize;

        public Logger(StoreEventDelegate<TState, TAction> next, Func<object, string> serialize)
        {
            _next = next;
            _serialize = serialize;
        }

        public TState InvokeAsync(TState state, TAction action)
        {
            Console.WriteLine("Class Logger state before action: {0}", _serialize(state));
            Console.WriteLine("Class Logger action: {0}", _serialize(action));
            var newState = _next(state, action);
            Console.WriteLine("Class Logger state after action: {0}", _serialize(newState));
            return newState;
        }
    }
}
