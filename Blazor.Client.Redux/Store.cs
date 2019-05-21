using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blazor.Client.Redux
{
    public class Store<TState, TAction> : IDisposable
    {
        #region Fields
        private readonly Reducer<TState, TAction> _rootReducer;
        private readonly ReduxOptions<TState, TAction> _options;
        private IList<HistoricStateEntry<TState, object>> _past;
        private IList<HistoricStateEntry<TState, object>> _future;
        private IUriHelper _uriHelper;
        private Func<TState, string> _getLocation;
        private Func<string, TAction> _locationActionCreator;
        private string _currentLocation;
        private readonly object _syncRoot = new object();
        #endregion

        #region Properties
        /// <summary>
        /// The current state of the application.
        /// </summary>
        public TState State { get; private set; }
        /// <summary>
        /// Handler invoked when state changes.
        /// </summary>
        public event EventHandler OnStateChanged;
        #endregion

        public Store(TState initialState, Reducer<TState, TAction> rootReducer, ReduxOptions<TState, TAction> options)
        {
            _rootReducer = rootReducer;
            _options = options;

            State = initialState;

            _past = new List<HistoricStateEntry<TState, object>>();
            AddHistoricStateEntry(new HistoricStateEntry<TState, object>(initialState));
            _future = new List<HistoricStateEntry<TState, object>>();
        }

        public void Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                State = _rootReducer(State, action);
                AddHistoricStateEntry(new HistoricStateEntry<TState, object>(State, action));
                _future.Clear();
            }

            InvokeOnStateChanged(null);
        }

        private void InvokeOnStateChanged(EventArgs e)
        {
            EventHandler handler = OnStateChanged;
            handler?.Invoke(this, e);
        }

        #region State History Management
        /// <summary>
        /// Safely add a historic entry to the history stack while adhering to the limitations of the ReduxOptions.
        /// </summary>
        /// <param name="HistoricStateEntry">The entry to add to the history stack.</param>
        private void AddHistoricStateEntry(HistoricStateEntry<TState, object> HistoricStateEntry)
        {
            if (_options.MaxHistoricalRecords == null)
            {
                return;
            }

            if (_past.Count >= _options.MaxHistoricalRecords + 1)
            {
                _past.RemoveAt(0);
            }
            _past.Add(HistoricStateEntry);
        }

        /// <summary>
        /// Revert state to a previous one.
        /// </summary>
        /// <param name="steps">The number of states (or actions dispatched) since the desired state.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough steps recorded to rewind as far as requested.</exception>
        public void Rewind(int steps)
        {
            // rewind 1 more than steps to account for current state being first in _past
            if (_past.Count < steps + 2)
            {
                throw new ArgumentOutOfRangeException(nameof(steps), "Cannot rewind more steps than are recorded.");
            }
            else
            {
                foreach (int step in Enumerable.Range(1, steps + 1))
                {
                    _future.Add(_past.ElementAt(_past.Count - 1));
                    _past.RemoveAt(_past.Count - 1);
                }

                lock (_syncRoot)
                {
                    State = _past.ElementAt(_past.Count - 1).State;
                }

                InvokeOnStateChanged(null);
            }
        }

        /// <summary>
        /// Return to a state that has been reverted.
        /// </summary>
        /// <param name="steps">The number of states (or actions) to replay.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when there are not enough steps reverted to fastforward as far as requested.</exception>
        public void FastForward(int steps)
        {
            if (_future.Count < steps)
            {
                throw new ArgumentOutOfRangeException(nameof(steps), "Cannot fast forward more steps than are recorded (dispatching an action erases possible futures).");
            }
            else
            {
                foreach (int step in Enumerable.Range(1, steps))
                {
                    AddHistoricStateEntry(_future.ElementAt(_future.Count - 1));
                    _future.RemoveAt(_future.Count - 1);
                }

                lock (_syncRoot)
                {
                    State = _past.ElementAt(_past.Count - 1).State;
                }

                InvokeOnStateChanged(null);
            }
        }
        #endregion

        #region Location Management
        /// <summary>
        /// Adds location synchronization to the store. The extent of the synchronization (two-way vs one-way) depends on the options set in the store.
        /// </summary>
        /// <param name="uriHelper">The application's UriHelper.</param>
        internal void InitializeLocationSynchronization(IUriHelper uriHelper)
        {
            if (_uriHelper == null && uriHelper != null && !_options.SuppressLocationSynchronization)
            {
                lock (_syncRoot)
                {
                    _uriHelper = uriHelper;
                    _uriHelper.OnLocationChanged += SynchronizeStateLocationWithUri;

                    _getLocation = _options.GetLocation ?? GetLocationFallback;
                    _locationActionCreator = _options.LocationActionCreator ?? LocationActionCreatorFallback;

                    OnStateChanged += SynchronizeUriLocationWithState;
                }

                SynchronizeStateLocationWithUri(null, _uriHelper.GetAbsoluteUri());
            }
        }

        private string GetLocationFallback(TState state) => _currentLocation;
        private TAction LocationActionCreatorFallback(string uri) => (TAction)(object)new ChangeLocationAction(uri);

        private void SynchronizeStateLocationWithUri(object sender, string newAbsoluteUri)
        {
            if (newAbsoluteUri != _currentLocation && _locationActionCreator != null)
            {
                lock (_syncRoot)
                {
                    _currentLocation = newAbsoluteUri;
                }

                Dispatch(_locationActionCreator(newAbsoluteUri));
            }
        }

        private void SynchronizeUriLocationWithState(object sender, EventArgs e)
        {
            if (_getLocation != null && _uriHelper != null)
            {
                var newLocation = _getLocation(State);
                if (newLocation != null)
                {
                    if (newLocation != _currentLocation)
                    {
                        lock (_syncRoot)
                        {
                            _currentLocation = newLocation;
                        }

                        _uriHelper.NavigateTo(newLocation);
                    }
                }
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false;  // to detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // free managed resources
                    if (_uriHelper != null)
                    {
                        _uriHelper.OnLocationChanged -= SynchronizeStateLocationWithUri;
                        OnStateChanged -= SynchronizeUriLocationWithState;
                    }
                }

                // free unmanaged resources (unmanaged objects)
                // set large fields to null

                disposedValue = true;
            }
        }

        // override a finalizer if Dispose(bool disposing) above has code to free unmanaged resources
        // ~Store()
        // {
        //   // do not change this code; put cleanup code in Dispose(bool disposing) above
        //   Dispose(false);
        // }

        void IDisposable.Dispose()
        {
            // do not change this code; put cleanup code in Dispose(bool disposing) above
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
