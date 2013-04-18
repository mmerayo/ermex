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
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.Bus.Synchronisation.Dialogs.HandledByMessageQueue;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Component;
using ermeX.Entities.Entities;
using ermeX.Threading.Queues;

namespace ermeX.Biz
{
	//TODO: THIS AND DIALOGSMANAGER SHOULD ENCAPSULATE THE CONNECTION FUNCTIONALLITY IN A PACKAGE AND BE EXTENSIBLE HIDING THE PROTOCOL
    internal sealed class ComponentManager : IComponentManager
    {
        private readonly IMessageDistributor _messageDistributor;
        private readonly IRegisterComponents _componentsRegister;
        private readonly IMessageSubscribersDispatcher _subscribersDispatcher;
        private readonly IReceptionMessageDistributor _receptionMessageDistributor;
        private readonly IUpdatePublishedServiceMessageHandler _updatePublishedServiceMessageHandler;
        private readonly IUpdateSuscriptionMessageHandler _subscriptionsUpdater;

        [Inject]
        public ComponentManager(IBizSettings settings, 
            IMessagePublisher publisher,
            IMessageListener listener,
	                                           IMessageDistributor messageDistributor,

                                IDialogsManager dialogsManager,
                                ICanReadComponents componentReader,
                                ICanWriteComponents componentWriter,
                                IStatusManager statusManager,
                                IRegisterComponents componentsRegister,
                                IMessageSubscribersDispatcher subscribersDispatcher,
                                IReceptionMessageDistributor receptionMessageDistributor,
                                IUpdatePublishedServiceMessageHandler updatePublishedServiceMessageHandler,
                                IUpdateSuscriptionMessageHandler subscriptionsUpdater)
        {
            _messageDistributor = messageDistributor;
            _componentsRegister = componentsRegister;
            _subscribersDispatcher = subscribersDispatcher;
            _receptionMessageDistributor = receptionMessageDistributor;
            _updatePublishedServiceMessageHandler = updatePublishedServiceMessageHandler;
            _subscriptionsUpdater = subscriptionsUpdater;
            if (settings == null) throw new ArgumentNullException("settings");
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (dialogsManager == null) throw new ArgumentNullException("dialogsManager");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            Settings = settings;
            Publisher = publisher;
            Listener = listener;
            DialogsManager = dialogsManager;
            ComponentReader = componentReader;
            ComponentWriter = componentWriter;
            StatusManager = statusManager;

            
        }

        private IBizSettings Settings { get; set; }
        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private IDialogsManager DialogsManager { get; set; }
        private ICanReadComponents ComponentReader { get; set; }
        private ICanWriteComponents ComponentWriter { get; set; }
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ComponentManager).FullName);
        private IStatusManager _statusManager;

        private IStatusManager StatusManager
        {
            get { return _statusManager; }
            set
            {
                if (_statusManager != null)
                    _statusManager.StatusChanged -= _statusManager_StatusChanged;

                _statusManager = value;
                _statusManager.StatusChanged += _statusManager_StatusChanged;
            }
        }

        private void _statusManager_StatusChanged(object sender, ComponentStatus newStatus)
        {
            ComponentWriter.SetComponentRunningStatus(Settings.ComponentId, newStatus, true);
            //TODO: remove second param when fixed, the exchanged definitions are set to false for all components at some point

            switch (newStatus)
            {
                case ComponentStatus.Stopped:
                    break;
                case ComponentStatus.Starting:
                    break;
                case ComponentStatus.Running:
                    DialogsManager.NotifyCurrentStatus();

                    DialogsManager.ExchangeDefinitions();
                    Logger.Trace(x => x("Component: {0} exchanged definitions", Settings.ComponentId));
                    break;
                case ComponentStatus.Stopping:
                    try
                    {
                        DialogsManager.NotifyCurrentStatus();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(
                            x =>
                            x("Could not notify stopping status to all components. Component: {0} Exception: {1}",
                              Settings.ComponentId, ex));
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("newStatus");
            }
        }



        #region IComponentStarter Members

        public void Start()
        {
            try
            {
                _componentsRegister.CreateLocalComponent(Settings.TcpPort);

                StatusManager.CurrentStatus = ComponentStatus.Starting;
                Logger.Trace(x => x("Component: {0} is STARTING", Settings.ComponentId));

                //TODO:THIS TO BE all injected from IStartable, the start to not to depend on the order as it does now
                Listener.Start();
                Publisher.Start();
                _receptionMessageDistributor.Start();
                _subscribersDispatcher.Start();
                _subscriptionsUpdater.Start();
                _updatePublishedServiceMessageHandler.Start();
                _messageDistributor.Start();
                DialogsManager.Start();
                DialogsManager.JoinNetwork();
                StatusManager.CurrentStatus = ComponentStatus.Running;
                //here so it accepts requests, see status changed handler above
                Logger.Trace(x => x("Component: {0} is RUNNING", Settings.ComponentId));

                IEnumerable<AppComponent> appComponents =
                    ComponentReader.FetchOtherComponentsNotExchangedDefinitions();

                DialogsManager.EnsureDefinitionsAreExchanged(appComponents);

            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Component Exception on Start", ex));
                throw;
            }
        }

        #endregion
    }
}