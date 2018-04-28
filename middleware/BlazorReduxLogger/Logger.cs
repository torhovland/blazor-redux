using BlazorRedux;
using System;
using System.Threading.Tasks;

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

        public Task InvokeAsync(TState state, TAction action)
        {
            Console.WriteLine("State before action: {0}", _serialize(state));
            Console.WriteLine("Action: {0}", _serialize(action));
            _next(state, action);
            Console.WriteLine("State after action: {0}", _serialize(state));
            return Task.CompletedTask;
        }
    }
}
