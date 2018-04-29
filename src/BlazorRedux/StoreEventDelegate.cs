using System.Threading.Tasks;

namespace BlazorRedux
{
    public delegate TState StoreEventDelegate<TState, TAction>(TState state, TAction action);
}
