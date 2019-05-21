using System;

namespace Blazor.Server.Redux
{
    public class ReduxOptions<TState, TAction>
    {
        /// <summary>
		/// The maximum number of past states to keep a record of.
        /// Set to null to not use record keeping.
        /// Value is 20 by default.
		/// </summary>
		public int? MaxHistoricalRecords { get; set; }
        /// <summary>
        /// A function that creates an action to be dispatched when the URI changes. 
        /// If not null, this replaces (uri) => new ChangeLocationAction(uri).
        /// Assign this if ChangeLocationAction does not fit your application's convention standards, or if TAction is not IAction and you would still like an action to be dispatched when the URI changes.
        /// </summary>
        public Func<string, TAction> LocationActionCreator { get; set; }
        /// <summary>
        /// When true the UriHelper does not attempt to stay synchronized with state, and no action gets dispatched when the URI changes.
        /// </summary>
        public bool SuppressLocationSynchronization { get; set; }

        public ReduxOptions()
        {
            // Defaults
            MaxHistoricalRecords = 20;
            LocationActionCreator = null;
            SuppressLocationSynchronization = false;
        }
    }
}
