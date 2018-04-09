namespace BlazorRedux
{
    public delegate TAction Dispatcher<TAction>(TAction action);
    public delegate TState Reducer<TState, in TAction>(TState previousState, TAction action);
}
