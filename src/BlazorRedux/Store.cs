using System;
using System.Threading.Tasks;

namespace BlazorRedux
{
    public class Store<TState, TAction>
    {
        private readonly Func<TState, TAction, TState> _reducer;
        private readonly object _syncRoot = new object();

        public Store(Func<TState, TAction, TState> reducer, TState initialState = default(TState))
        {
            Console.WriteLine("Working Store constructor.");
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

        public object Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                State = _reducer(State, action);
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