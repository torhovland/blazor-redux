using Microsoft.AspNetCore.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorRedux
{
    public class StoreBuilder<TState, TAction> : IStoreBuilder<TState, TAction>
    {
        private readonly IList<Func<StoreEventDelegate<TState, TAction>, StoreEventDelegate<TState, TAction>>> _components = new List<Func<StoreEventDelegate<TState, TAction>, StoreEventDelegate<TState, TAction>>>();

        public StoreEventDelegate<TState, TAction> Build(Store<TState, TAction> store)
        {
            StoreEventDelegate<TState, TAction> app = async (TState state,  TAction action) =>
            {
                Console.WriteLine("State before Store InvokeAsync: {0}", JsonUtil.Serialize(state));
                var _state = state;
                await store.Invoke(ref state, action);
                Console.WriteLine("State after InvokeAsync should change to match 'State has changed inside InvokeAsync' above: {0}", JsonUtil.Serialize(state));
            };

            foreach (var component in _components.Reverse())
            {
                app = component(app);
            }

            return app;
        }

        public IStoreBuilder<TState, TAction> Use(Func<StoreEventDelegate<TState, TAction>, StoreEventDelegate<TState, TAction>> middleware)
        {
            _components.Add(middleware);
            return this;
        }
    }
}
