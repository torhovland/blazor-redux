using System;
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
        }

        public TState State { get; private set; }

        public event EventHandler Change;

        private void OnChange(EventArgs e)
        {
            var handler = Change;
            handler?.Invoke(this, e);
        }

        public void Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                State = _reducer(State, action);
            }

            OnChange(null);
        }

        public Task DispatchAsync(AsyncActionsCreator<TState, TAction> actionsCreator)
        {
            return actionsCreator(Dispatch, State);
        }
    }
}