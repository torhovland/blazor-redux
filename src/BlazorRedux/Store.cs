using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRedux
{
    public delegate object Dispatcher(object action);
    public delegate TState Reducer<TState>(TState previousState, object action);
    public delegate Task AsyncActionsCreator<in TState>(Dispatcher dispatcher, TState state);
    
    public class Store<TState>
    {
        private readonly object _syncRoot = new object();
        private TState _lastState;
        private readonly Reducer<TState> _reducer;

        public Store(Reducer<TState> reducer, TState initialState = default(TState))
        {
            _reducer = reducer;
            _lastState = initialState;
        }

        public TState State => _lastState;

        public event EventHandler Change;

        void OnChange(EventArgs e)
        {
            EventHandler handler = Change;
            handler?.Invoke(this, e);
        }

        public object Dispatch(object action)
        {
            lock (_syncRoot)
            {
                _lastState = _reducer(_lastState, action);
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
