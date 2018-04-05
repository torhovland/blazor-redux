using System;
using System.Threading.Tasks;

namespace BlazorRedux
{
    public class Store<TState>
    {
        private readonly Reducer<TState> _reducer;
        private readonly object _syncRoot = new object();

        public Store(Reducer<TState> reducer, TState initialState = default(TState))
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

        public object Dispatch(object action)
        {
            lock (_syncRoot)
            {
                State = _reducer(State, action);
            }

            OnChange(null);

            return action;
        }

        public Task DispatchAsync(AsyncActionsCreator<TState> actionsCreator)
        {
            return actionsCreator(Dispatch, State);
        }
    }
}