using Microsoft.AspNetCore.Components;
using System;

namespace Blazor.Client.Redux
{
    public class ReduxComponent<TState, TAction> : ComponentBase, IDisposable
    {
        [Inject] protected Store<TState, TAction> Store { get; set; }
        [Inject] protected IUriHelper UriHelper { get; set; }
        protected TState State => Store.State;

        protected override void OnInit()
        {
            Store.InitializeLocationSynchronization(UriHelper);
            Store.OnStateChanged += OnStateChangedHandler;
            base.OnInit();
        }

        protected void Dispatch(TAction action) => Store.Dispatch(action);

        private void OnStateChangedHandler(object sender, EventArgs e) => StateHasChanged();

        #region IDisposable Support
        private bool disposedValue = false; // to detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    Store.OnStateChanged -= OnStateChangedHandler;
                }

                // free unmanaged resources (unmanaged objects)
                // set large fields to null.

                disposedValue = true;
            }
        }

        // override a finalizer if Dispose(bool disposing) above has code to free unmanaged resources
        // ~ReduxComponent()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
