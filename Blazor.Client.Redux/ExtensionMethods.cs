using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Client.Redux
{
    public static class ExtensionMethods
    {
        /// <summary>
		/// Add a store interface singleton to the application's service collection.
		/// </summary>
		/// <typeparam name="TState">The type of the state.</typeparam>
		/// <typeparam name="TAction">The type of action that the state's reducers take.</typeparam>
		/// <param name="initialState">The initial state.</param>
		/// <param name="rootReducer">The top-level reducer for the state.</param>
		/// <param name="options">Options determining how redux handles certain functions.</param>
		/// <returns>The singleton store assigned.</returns>
        public static Store<TState, TAction> AddReduxStore<TState, TAction>(
            this IServiceCollection configure,
            TState initialState,
            Reducer<TState, TAction> rootReducer,
            ReduxOptions<TState, TAction> options = null)
        {
            var store = new Store<TState, TAction>(initialState, rootReducer, options);
            configure.AddSingleton<Store<TState, TAction>>(store);
            return store;
        }
    }
}
