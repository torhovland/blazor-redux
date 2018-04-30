using BlazorRedux;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using System;

namespace BlazorReduxLogger
{
    public class Logger<TState, TAction>
    {
        private readonly StoreEventDelegate<TState, TAction> _next;

        public Logger(StoreEventDelegate<TState, TAction> next)
        {
            _next = next;
        }

        public TState Invoke(TState state, TAction action)
        {
            Console.WriteLine("Class Logger state before action: {0}", JsonUtil.Serialize(state));
            Console.WriteLine("Class Logger action: {0}", JsonUtil.Serialize(action));
            //Logger<TState, TAction>.Log(state);
            var newState = _next(state, action);
            Console.WriteLine("Class Logger state after action: {0}", JsonUtil.Serialize(newState));
            return newState;
        }

        public static void Log(object message)
        {
            RegisteredFunction.Invoke<bool>("Logger.Log", JsonUtil.Serialize(message));
        }
    }
}
