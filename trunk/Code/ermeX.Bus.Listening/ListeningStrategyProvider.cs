// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Common.Logging;
using Ninject;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Transport.Interfaces;

namespace ermeX.Bus.Listening
{
    internal class ListeningStrategyProvider : IListeningStrategyProvider
    {
        private bool _started;

        [Inject]
        public ListeningStrategyProvider(IBusSettings settings,
                                         InternalMessageHandler internalMessageHandler,
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

        private InternalMessageHandler InternalMessageHandler { get; set; }
        private IConnectivityManager ConnectivityManager { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
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
            //TODO:if injection cannot be singleton remove singleton feature
            InternalMessageHandler.StartWorkers();

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
            ConnectivityManager.RegisterHandler(InternalMessageHandler.OperationIdentifier, InternalMessageHandler);
        }
    }
}