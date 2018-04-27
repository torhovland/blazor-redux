using BlazorRedux;
using System;
using System.Threading.Tasks;

namespace BlazorReduxLogger
{
    public class Logger<TState, TAction>
    {
        private readonly StoreEventDelegate<TState, TAction> _next;

        public Logger(StoreEventDelegate<TState, TAction> next)
        {
            _next = next;
        }

        public Task InvokeAsync(TState state, TAction action)
        {
            Console.WriteLine("State before action: ", state);
            Console.WriteLine("Action: ", action);
            _next(state, action);
            Console.WriteLine("State after action: ", state);
            return Task.CompletedTask;
        }
    }
}
