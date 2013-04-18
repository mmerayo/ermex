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
using System.Diagnostics;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
    internal sealed class HandshakeServiceHandler : IHandshakeService
    {
	    private readonly ICanWriteComponents _componentsWritter;
	    private readonly IRegisterComponents _componentsRegistrator;
	    private readonly ICanReadConnectivityDetails _connectivityReader;

	    [Inject]
        public HandshakeServiceHandler(IMessagePublisher publisher, IMessageListener listener,
                                       IComponentSettings settings,
			ICanReadComponents componentReader,
                                       ICanWriteComponents componentsWritter,
									   IStatusManager statusManager, IRegisterComponents componentsRegistrator,ICanReadConnectivityDetails connectivityReader)
        {
		    _componentsWritter = componentsWritter;
		    _componentsRegistrator = componentsRegistrator;
		    _connectivityReader = connectivityReader;
		    if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (settings == null) throw new ArgumentNullException("settings");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            Publisher = publisher;
            Listener = listener;
            Settings = settings;
	        ComponentReader = componentReader;
            
            StatusManager = statusManager;
        }


        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }
        private IComponentSettings Settings { get; set; }
	    private ICanReadComponents ComponentReader { get; set; }

		private static readonly ILog Logger = LogManager.GetLogger(typeof(HandshakeServiceHandler).FullName);
        private IStatusManager StatusManager { get; set; }

        //this handler id is static

        #region IHandshakeService Members

        public MyComponentsResponseMessage RequestJoinNetwork(JoinRequestMessage message)
        {
            //should call before ComponentIsReady
            //StatusManager.WaitIsRunning();
            try
            {
                Logger.Trace(x=>x("RequestJoinNetwork RECEIVED on {0} from {1}", Settings.ComponentId,
                                           message.SourceComponentId));

                _componentsRegistrator.CreateRemoteComponent(message.SourceComponentId,message.SourceIp,message.SourcePort);

                //prepare result
                var componentsDatas = new List<Tuple<AppComponent, ConnectivityDetails>>();

                var components = new List<AppComponent>(ComponentReader.FetchAll());

                foreach (var appComponent in components)
                {
                    appComponent.ComponentExchanges = null; //To not to share the local configurations
                    var tuple = new Tuple<AppComponent, ConnectivityDetails>(appComponent,_connectivityReader.Fetch(appComponent.ComponentId));
                    componentsDatas.Add(tuple);
                }
                var result = new MyComponentsResponseMessage(message.SourceComponentId, componentsDatas);

                Logger.Trace(x=>x("RequestJoinNetwork HANDLED on {0} from {1}", Settings.ComponentId,
                                           message.SourceComponentId));
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "RequestJoinNetwork: Error while handling request. Exception: {0}", ex));
                throw ex;
            }
        }

        public ComponentStatus ExchangeComponentStatus(Guid sourceComponentId, ComponentStatus status)
        {
            try
            {
                Debug.Assert(sourceComponentId != Settings.ComponentId);
                var item = ComponentReader.Fetch(sourceComponentId);
                if (item == null)
                    throw new InvalidOperationException(
                        string.Format("The component {0} didnt exist previously. Use RequestJoinNetwork before this.",
                                      sourceComponentId));

                item.IsRunning = status == ComponentStatus.Running;

                _componentsWritter.Save(item);
                Logger.Trace(
                    string.Format(
                        "ComponentStatus HANDLED on {0} from {1} Input.IsRunning: {2} Output.IsRunning: {3}",
                        Settings.ComponentId,
                        sourceComponentId, item.IsRunning, StatusManager.CurrentStatus));

                return StatusManager.CurrentStatus;
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "ComponentStatus: Error while handling request. {0}", ex));
                throw ex;
            }
        }

        public bool CanExchangeDefinitions(Guid componentId)
        {
            try
            {
                //TODO: transactional
                bool result=false;
                AppComponent component = ComponentReader.Fetch(componentId);

                if (componentId != component.ComponentOwner && componentId != component.ComponentId)
                    throw new InvalidOperationException(string.Format("the componentId: {0} is not valid as an exchanger for component:{1} owned by :{2}", componentId,component.ComponentId,component.ComponentOwner));
                if (component.ComponentExchanges == componentId)
                    result = true;
                else if(component.ComponentExchanges==Settings.ComponentId)
                    result = false;
                else
                {
                    component.ComponentExchanges = componentId;
                    component.ExchangedDefinitions = false;
                    _componentsWritter.Save(component);
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "ComponentStatus: Error while handling request CanExchangeDefinitions. {0}", ex));
                throw ex;
            }
        }

        public void DefinitionsExchanged(Guid componentId)
        {
            try
            {
                //TODO: transactional
                AppComponent component = ComponentReader.Fetch(componentId);
                component.ExchangedDefinitions = true;
                component.ComponentExchanges = null;
                _componentsWritter.Save(component);

                StatusManager.SyncEvents.SignalEvent(
                        ConfigurationManagement.Status.StatusManager.GlobalSync.GlobalEvents.DefinitionsExchanged,
                        component.ComponentId);
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "ComponentStatus: Error while handling request DefinitionsExchanged. {0}", ex));
                throw ex;
            }
        }

        #endregion
    }
}