// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
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