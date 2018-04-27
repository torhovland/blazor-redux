using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorRedux
{
    public interface IStoreBuilder<TState, TAction>
    {
        IStoreBuilder<TState, TAction> Use(Func<StoreEventDelegate<TState, TAction>, StoreEventDelegate<TState, TAction>> middleware);

        StoreEventDelegate<TState, TAction> Build(Store<TState, TAction> store);
    }
}
