namespace BlazorRedux
{
    public delegate void Dispatcher<in TAction>(TAction action);
    public delegate TState Reducer<TState, in TAction>(TState previousState, TAction action);
    public delegate TState StoreEventDelegate<TState, TAction>(TState state, TAction action);
}
