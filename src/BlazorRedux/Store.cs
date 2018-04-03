using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorRedux
{
    public class Store<TModel>
    {
        public Store(Func<TModel> initialModel)
        {
            Mdl = initialModel();
        }

        public TModel Mdl { get; private set; }
    }
}
