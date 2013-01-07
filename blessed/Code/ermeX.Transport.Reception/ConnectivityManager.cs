// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Ninject;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.LayerInterfaces;

namespace ermeX.Transport.Reception
{
    //TODO: CHANGE CLASS NAME OR SPLIT
    internal class ConnectivityManager : IConnectivityManager
    {
        private readonly List<IServer> _servers = new List<IServer>();
        private readonly object _syncLock = new object();
        private volatile bool _serversLoaded;

        [Inject]
        public ConnectivityManager(IProxyProvider proxyProvider, IEnumerable<IConcreteServiceLoader> serversLoader)
        {
            if (proxyProvider == null) throw new ArgumentNullException("proxyProvider");
            if (serversLoader == null) throw new ArgumentNullException("serversLoader");

            ProxyProvider = proxyProvider;
            ServersLoaders = serversLoader;
        }

        private IEnumerable<IConcreteServiceLoader> ServersLoaders { get; set; }
        private IProxyProvider ProxyProvider { get; set; }

        #region IConnectivityManager Members

        public List<IEndPoint> GetClientProxiesForComponent(Guid destinationComponent)
        {
            return ProxyProvider.GetClientProxies(destinationComponent);
        }

        public void LoadServers()
        {
            if (!_serversLoaded)
                lock (_syncLock)
                    if (!_serversLoaded)
                    {
                        foreach (var loader in ServersLoaders)
                        {
                            var loadServers = loader.LoadServers();
                            _servers.AddRange(loadServers);
                        }
                        _serversLoaded = true;
                    }
        }

        public void RegisterHandler(Guid operationIdentifier, IServiceHandler internalMessageHandler)
        {
            foreach (var server in _servers)
            {
                server.RegisterHandler(operationIdentifier, internalMessageHandler);
            }
        }

        public void StartListening()
        {
            foreach (var server in _servers)
            {
                server.StartListening();
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
                    foreach (var server in _servers)
                    {
                        server.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        #endregion
    }
}