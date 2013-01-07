// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject;

namespace ermeX.Bus.Listening
{
    internal class ListeningManager : IListeningManager
    {
        private readonly object _syncLock = new object();
        private volatile bool _serversStarted;

        [Inject]
        public ListeningManager(IListeningStrategyProvider serversProvider)
        {
            if (serversProvider == null) throw new ArgumentNullException("serversProvider");
            ServersProvider = serversProvider;
        }

        private IListeningStrategyProvider ServersProvider { get; set; }

        #region IListeningManager Members

        public void StartServers(Action<Guid, object> onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException("onMessageReceived");
            if (!_serversStarted)
                lock (_syncLock)
                    if (!_serversStarted)
                    {
                        ServersProvider.Initialize();
                        ServersProvider.StartListening(onMessageReceived);
                        _serversStarted = true;
                    }
        }

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ServersProvider.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}