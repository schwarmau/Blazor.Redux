using Microsoft.Extensions.DependencyInjection;
using System;

namespace Blazor.Client.Redux
{
    public static class ExtensionMethods
    {
        /// <summary>
		/// Add a store singleton to the application's service collection.
		/// </summary>
		/// <typeparam name="TState">The type of the state.</typeparam>
		/// <typeparam name="TAction">The type of action that the state's reducers take.</typeparam>
		/// <param name="initialState">The initial state.</param>
		/// <param name="rootReducer">The top-level reducer for the state.</param>
		/// <param name="configure">Options configuration determining how redux handles certain functions.</param>
		/// <returns>The singleton store assigned.</returns>
        public static Store<TState, TAction> AddReduxStore<TState, TAction>(
            this IServiceCollection services,
            TState initialState,
            Reducer<TState, TAction> rootReducer,
            Action<ReduxOptions<TState, TAction>> configure = null)
        {
            ReduxOptions<TState, TAction> options = new ReduxOptions<TState, TAction>();
            configure?.Invoke(options);
            Store<TState, TAction> store = new Store<TState, TAction>(initialState, rootReducer, options);
            services.AddSingleton<Store<TState, TAction>>(store);
            return store;
        }
    }
}
