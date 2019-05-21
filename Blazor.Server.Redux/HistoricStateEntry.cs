using System;

namespace Blazor.Server.Redux
{
    public class HistoricStateEntry<TState, TAction>
    {
        public TState State { get; }
        public TAction Action { get; }
        public DateTime Time { get; }

        public HistoricStateEntry(TState state, TAction action = default(TAction))
        {
            State = state;
            Action = action;
            Time = DateTime.UtcNow;
        }
    }
}
