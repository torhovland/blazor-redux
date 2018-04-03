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

        public event EventHandler Change;

        public void OnChange(EventArgs e)
        {
            EventHandler handler = Change;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
