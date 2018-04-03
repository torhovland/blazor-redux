using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorRedux
{
    public class ReduxComponent<TModel> : BlazorComponent where TModel : IModel
    {
        [Inject]
        public Store<TModel> Store { get; set; }

        public TModel Mdl => Store.Mdl;

        public async Task ProcessAsync(object action)
        {
            await Mdl.ProcessAsync(action);
        }
    }
}
