namespace Blazor.Server.Redux
{
    public delegate TState Reducer<TState, in TAction>(TState previousState, TAction action);
}
