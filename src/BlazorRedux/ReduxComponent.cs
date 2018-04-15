using System;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Services;

namespace BlazorRedux
{
    public class ReduxComponent<TState, TAction> : BlazorComponent, IDisposable
    {
        [Inject] public Store<TState, TAction> Store { get; set; }
        [Inject] private IUriHelper UriHelper { get; set; }

        public TState State => Store.State;

        public RenderFragment ReduxDevTools;

        public void Dispose()
        {
            Store.Change -= OnChangeHandler;
        }

        protected override void OnInit()
        {
            Store.Init(UriHelper);
            Store.Change += OnChangeHandler;

            ReduxDevTools = builder =>
            {
                var seq = 0;
                builder.OpenComponent<ReduxDevTools>(seq);
                builder.CloseComponent();
            };
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