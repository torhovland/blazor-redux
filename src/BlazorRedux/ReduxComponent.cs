using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorRedux
{
    public class ReduxComponent<TState, TAction> : BlazorComponent, IDisposable
    {
        [Inject] public Store<TState, TAction> Store { get; set; }

        public TState State => Store.State;

        public void Dispose()
        {
            Store.Change -= OnChangeHandler;
        }

        protected override void OnInit()
        {
            Store.Change += OnChangeHandler;
        }

        private void OnChangeHandler(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        public void Dispatch(TAction action)
        {
            Store.Dispatch(action);
        }
    }
}