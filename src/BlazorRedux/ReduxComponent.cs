using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorRedux
{
    public class ReduxComponent<TState> : BlazorComponent, IDisposable
    {
        [Inject]
        public Store<TState> Store { get; set; }

        public TState State => Store.State;

        protected override void OnInit()
        {
            Store.Change += OnChangeHandler;
        }

        public void Dispose()
        {
            Store.Change -= OnChangeHandler;
        }

        private void OnChangeHandler(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        public void Dispatch(object action)
        {
            Store.Dispatch(action);
        }

        public Task DispatchAsync(AsyncActionsCreator<TState> actionsCreator)
        {
            return Store.DispatchAsync(actionsCreator);
        }
    }
}
