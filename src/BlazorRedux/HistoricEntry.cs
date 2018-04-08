using System;

namespace BlazorRedux
{
    public class HistoricEntry<TState, TAction>
    {
        public HistoricEntry(TState state, TAction action = default(TAction))
        {
            State = state;
            Action = action;
            Time = DateTime.UtcNow;
        }

        public TState State { get; }
        public TAction Action { get; }
        public DateTime Time { get; }
    }
}
