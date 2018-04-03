using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorRedux
{
    public class Store<TModel>
    {
        public Store(Func<TModel> init)
        {
            Mdl = init();
        }

        public TModel Mdl { get; private set; }
    }
}
