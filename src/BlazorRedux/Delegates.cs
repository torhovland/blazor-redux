using System.Threading.Tasks;

namespace BlazorRedux
{
    public delegate TAction Dispatcher<TAction>(TAction action);

    public delegate TState Reducer<TState, in TAction>(TState previousState, TAction action);

    public delegate Task AsyncActionsCreator<in TState, TAction>(Dispatcher<TAction> dispatcher, TState state);
}
