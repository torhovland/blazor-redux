using System.Threading.Tasks;

namespace BlazorRedux
{
    public interface IMiddleware<TState, TAction>
    {
        Task InvokeAsync(TState state, TAction action, StoreEventDelegate<TState, TAction> next);
    }
}
