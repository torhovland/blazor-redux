using System.Threading.Tasks;

namespace BlazorRedux
{
    public delegate Task StoreEventDelegate<TState, TAction>(TState state, TAction action);
}
