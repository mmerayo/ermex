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
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Transport.Interfaces;

namespace ermeX.Bus.Listening
{
    /// <summary>
    /// Loads services handlers and starts listening
    /// </summary>
    internal class ListeningStrategyProvider : IListeningStrategyProvider
    {
        private bool _started;

        [Inject]
        public ListeningStrategyProvider(IBusSettings settings,
                                         ReceptionMessageHandler internalMessageHandler,
                                         IConnectivityManager connectivityManager)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (internalMessageHandler == null) throw new ArgumentNullException("internalMessageHandler");
            if (connectivityManager == null) throw new ArgumentNullException("connectivityManager");
            Settings = settings;
            InternalMessageHandler = internalMessageHandler;
            ConnectivityManager = connectivityManager;
        }

        private IBusSettings Settings { get; set; }

        private ReceptionMessageHandler InternalMessageHandler { get; set; }
        private IConnectivityManager ConnectivityManager { get; set; }
		private static readonly ILogger Logger = LogManager.GetLogger(typeof(ListeningStrategyProvider).FullName);
        #region IListeningStrategyProvider Members

        public void Initialize()
        {
            ConnectivityManager.LoadServers();
            LoadServiceHandlers();
        }


        public void StartListening(Action<Guid, object> onMessageReceived)
        {
            if (_started)
                throw new InvalidOperationException("Servers already started");

            Logger.Trace("ListeningStrategyProvider.StartListening: Component: " + Settings.ComponentId);
            InternalMessageHandler.RegisterSuscriber(onMessageReceived);

            ConnectivityManager.StartListening();
            _started = true;
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
                    //TODO: LOG THREADS DISPOSED
                }

                _disposed = true;
            }
        }

        #endregion

        private void LoadServiceHandlers()
        {
            ConnectivityManager.RegisterHandler(ReceptionMessageHandler.OperationIdentifier, InternalMessageHandler);
        }
    }
}