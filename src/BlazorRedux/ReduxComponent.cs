using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorRedux
{
    public class ReduxComponent<TModel> : BlazorComponent, IDisposable where TModel : IModel
    {
        [Inject]
        public Store<TModel> Store { get; set; }

        public TModel Mdl => Store.Mdl;

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

        public async Task ProcessAsync(object action)
        {
            await Mdl.ProcessAsync(action);
            Store.OnChange(null);
        }
    }
}
