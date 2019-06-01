## Forked from torhovland's blazor-redux repository.

The changes here are first and foremost an upgrade to the latest versions of Blazor.

## Blazor is now split into two hosting models: client-side and server-side.

### Revisions to both hosting models
#### 1. Configurable history management
 - I added a property to the ReduxOptions object that allows for configuring how many states to remember.
 - I added built-in functionality for undo and redo, maintaining the mannerisms originally used by torhovland ("Rewind" and "FastForward" to stay in line with "TimeTravel")
I decided to include this in the repo because of how small it is, and because it has no theoretical performance impact unless implemented.

#### 2. Modified location synchronization
One thing that bugged me with the source repo is that the way I've been working with the redux pattern, namely the conventions I've been following with actions, does not align with the NewLocationAction built into the library. I removed the option to add a location reducer (because it really wasn't needed) and I added the option to specify a location action creator. Instead of dispatching a ChangeLocationAction (formerly NewLocationAction), you can have it dispatch an action you have defined yourself. This is particularly useful in the case that you define your own action type to use instead of IAction.

I tried to keep the library light, and it would have been lighter without the location management. To that front, I tried several external implementations of location synchronization, but I could not figure out how to access the UriHelper outside of components, which meant I had to do things that felt overkill (one way involved using static properties, another way involved manipulating a layout component that applied to all pages so it wouldn't ever be disposed). The lightest-weight approach ended up being just keeping it in the library. If anyone has a better approach, feel free to make an issue explaining it and I will take the location synchronization out and include the solution in this readme.

You can opt not to use location synchronization either by using the SuppressLocationSynchronization option, or by not inheriting from ReduxComponent.

#### 3. Removed dev tools
They weren't implemented in a particularly lightweight way, and I never used them so I just took them out instead of modifying them.

### Client-side revisions
Client-side mostly stayed the same. It still targets the netcoreapp2.0 framework, and the source repo was compatible with client-side in the first place.

### Server-side revisions
 - Replaced netcoreapp2.0 dependencies with their netcoreapp3.0 equivalents. 
 - Modified the OnStateChanged handler to wrap the `StateHasChanged()` call in a try-catch.
 The reason being that the application is maintained server-side, so if the user refreshes the page, not only will the components not be disposed of, but also the store will not be disposed, so those OnStateChanged handlers will linger in components with no render tree.
 - Reduced location synchronization from two-way synchronization to one-way synchronization.
 Server-side Blazor operates in a completely different way from client-side, via SignalR connections. Because of this, the render cycle can be interrupted by the UriHelper trying to navigate, and that causes a whole bunch of problems. This means the UriHelper can't synchronize with state. Fortunately, all that *really* means is that you can't call `Dispatch(new ChangeLocationAction(uri))` to navigate. The flipside of that is still true, though, if you navigate the page (via UriHelper or simply with the forward and back buttons in the browser), a ChangeLocationAction will still be dispatched, and you can therefore still handle location changes in your reducers. You're just forced to use the UriHelper to navigate in your components.
 - Store now has scoped behavior instead of singleton behavior. This makes it behave more like it does in client-side blazor, otherwise multiple users accessing the app would share state.

## Some notes
I deliberately chose not to create an interface for the store, despite some of the benefits.
This was because I feel that users of this library should not be implementing their own stores, as the point of this library is providing one. The best way to encourage that behavior is to just not allow it. The other drawback is having to make the location synchronization function public. On it's own, that's not a problem, but it means anyone who did want to implement their own store would have to implement that; it just doesn't make sense as a contractual obligation set by the interface.

The reason I bring this up is to recommend a unit testing approach, because not having an interface makes unit testing a little trickier. As of the time of this writing, unfortunately the only way to inject services into components is through the [Inject] attribute, which means if those services are intended to be privately set then the only way (that I know) to set them in unit tests is to have the unit test class inherit from the component, provided the setter is at least protected.
 - In the case of the store, you can set the store with a simple newing up: `Store = new Store<TState, TAction>(TState initialState, Reducer<TState, TAction> rootReducer, ReduxOptions<TState, TAction> options);`.
 - To mock the retrieval of properties from state you can either declare a fleshed-out initial state with those properties, or you can inject selectors to your components.
 - To test if an action has been dispatched, set the root reducer to handle any action by setting it in a variable that you can test on: `rootReducer = (state, action) => { dispatchedAction = action; };`

## Using this library

In `Startup.cs`:
```
public void ConfigureServices(IServiceCollection services)
{
	services.AddReduxStore<ApplicationState, IAction>(yourInitialState, YourRootReducer, options =>
	{
		// configure
		// ex: MaxHistoricalRecords = 20
	});
}
```

Your components should inherit from `ReduxComponent<YourStateType, YourActionType>`:
```
public class MyComponent : ReduxComponent<MyState, IAction> { }
```

I recommend defining your own component base that all your components can inherit from:
```
public class MyComponentBase : ReduxComponent<ApplicationState, IAction>
{
	protected ApplicationState State => Store.State;
	protected void Dispatch(IAction action) => Store.Dispatch(action);
}
```
This has the benefit of modularity, so if you ever replace this library with another, or create your own store, all you have to do is uninherit ReduxComponent, inherit from the the default ComponentBase class, and redefine State and Dispatch.