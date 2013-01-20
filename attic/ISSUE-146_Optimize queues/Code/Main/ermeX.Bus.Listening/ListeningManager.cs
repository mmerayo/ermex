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