using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRedux
{
    public delegate object Dispatcher(object action);

    public delegate TState Reducer<TState>(TState previousState, object action);

    public delegate Task AsyncActionsCreator<in TState>(Dispatcher dispatcher, TState state);
}
