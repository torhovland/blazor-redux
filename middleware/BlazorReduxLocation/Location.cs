using BlazorRedux;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Services;
using System;

namespace BlazorReduxLocation
{
    public class Location<TState, TAction>
    {
        private readonly StoreEventDelegate<TState, TAction> _next;
        private readonly Store<TState, TAction> _store;

        public Location(StoreEventDelegate<TState, TAction> next, IUriHelper uriHelper, Store<TState, TAction> store)
        {
            _next = next;
            _store = store;
            uriHelper.OnLocationChanged += OnLocationChanged;
        }
        public TState Invoke(TState state, TAction action)
        {
            Console.WriteLine("Location action: {0}", JsonUtil.Serialize(action));
            Console.WriteLine("Location action type: {0}", action.GetType());
            if (typeof(NewLocationAction) == action.GetType()) Console.WriteLine("Location!");
            var newState = _next(state, action);
            return newState;
        }

        private void OnLocationChanged(object sender, string newAbsoluteUri)
        {
            Console.WriteLine("New location {0}", newAbsoluteUri);
            _store.Dispatch(new NewLocationAction { Location = newAbsoluteUri });
        }
    }
}
