using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Blazor;

namespace BlazorRedux
{
    public class Store<TState, TAction>
    {
        private readonly Reducer<TState, TAction> _reducer;
        private readonly object _syncRoot = new object();

        public Store(Reducer<TState, TAction> reducer, TState initialState = default(TState))
        {
            _reducer = reducer;
            State = initialState;
            History = new List<HistoricEntry<TState, TAction>>
            {
                new HistoricEntry<TState, TAction>(State)
            };
        }

        public TState State { get; private set; }

        public IList<HistoricEntry<TState, TAction>> History { get; }

        public event EventHandler Change;

        private void OnChange(EventArgs e)
        {
            var handler = Change;
            handler?.Invoke(this, e);
        }

        public TAction Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                State = _reducer(State, action);
                Console.WriteLine("Calling DevTools here.");
                DevToolsInterop.Log(action.ToString(), JsonUtil.Serialize(State));
                History.Add(new HistoricEntry<TState, TAction>(State, action));
            }

            OnChange(null);
            return action;
        }

        public void TimeTravel(TState state)
        {
            lock (_syncRoot)
            {
                State = state;
            }

            OnChange(null);
        }
    }
}