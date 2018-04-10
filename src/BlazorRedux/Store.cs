using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Blazor;

namespace BlazorRedux
{
    public class Store<TState, TAction> : IDisposable
    {
        private readonly Reducer<TState, TAction> _reducer;
        private readonly object _syncRoot = new object();

        public Store(Reducer<TState, TAction> reducer, TState initialState = default(TState))
        {
            _reducer = reducer;
            State = initialState;

            DevToolsInterop.TimeTravel += OnDevToolsTimeTravel;

            DevToolsInterop.Log("initial", JsonUtil.Serialize(State));

            History = new List<HistoricEntry<TState, TAction>>
            {
                new HistoricEntry<TState, TAction>(State)
            };
        }

        public void Dispose()
        {
            DevToolsInterop.TimeTravel -= OnDevToolsTimeTravel;
        }

        private void OnDevToolsTimeTravel(object sender, StringEventArgs e)
        {
            var state = JsonUtil.Deserialize<TState>(e.String);
            TimeTravel(state);
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