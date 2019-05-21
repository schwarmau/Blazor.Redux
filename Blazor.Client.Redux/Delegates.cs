namespace Blazor.Client.Redux
{
    public delegate TState Reducer<TState, in TAction>(TState previousState, TAction action);
}
