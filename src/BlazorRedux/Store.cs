using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                History.Add(new HistoricEntry<TState, TAction>(State, action));
            }

            OnChange(null);
            return action;
        }

        public Task DispatchAsync(AsyncActionsCreator<TState, TAction> actionsCreator)
        {
            return actionsCreator(Dispatch, State);
        }
    }
}