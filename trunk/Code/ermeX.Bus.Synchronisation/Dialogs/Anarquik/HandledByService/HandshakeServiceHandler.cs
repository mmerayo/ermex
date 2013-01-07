// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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

using ermeX.Entities.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
    internal sealed class HandshakeServiceHandler : IHandshakeService
    {
        [Inject]
        public HandshakeServiceHandler(IMessagePublisher publisher, IMessageListener listener,
                                       IComponentSettings settings,
                                       IAppComponentDataSource componentsDataSource,
                                       IConnectivityDetailsDataSource connectivityDataSource,
                                       IStatusManager statusManager
            ,IAutoRegistration autoRegistration)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (settings == null) throw new ArgumentNullException("settings");
            if (componentsDataSource == null) throw new ArgumentNullException("componentsDataSource");
            if (connectivityDataSource == null) throw new ArgumentNullException("connectivityDataSource");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            if (autoRegistration == null) throw new ArgumentNullException("autoRegistration");
            Publisher = publisher;
            Listener = listener;
            Settings = settings;
            ComponentsDataSource = componentsDataSource;
            ConnectivityDataSource = connectivityDataSource;
            StatusManager = statusManager;
            AutoRegistration = autoRegistration;
        }


        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }
        private IComponentSettings Settings { get; set; }
        private IAppComponentDataSource ComponentsDataSource { get; set; }
        private IConnectivityDetailsDataSource ConnectivityDataSource { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
        private IStatusManager StatusManager { get; set; }
        private IAutoRegistration AutoRegistration { get; set; }

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

                AutoRegistration.CreateRemoteComponentInitialSetOfData(message.SourceComponentId,message.SourceIp,message.SourcePort);

                //prepare result
                var componentsDatas = new List<Tuple<AppComponent, ConnectivityDetails>>();

                var components = new List<AppComponent>(ComponentsDataSource.GetAll());

                foreach (var appComponent in components)
                {
                    appComponent.ComponentExchanges = null; //To not to share the local configurations
                    var tuple = new Tuple<AppComponent, ConnectivityDetails>(appComponent,
                                                                             ConnectivityDataSource.GetByComponentId(
                                                                                 appComponent.ComponentId));
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
                var item = ComponentsDataSource.GetItemByField("ComponentId", sourceComponentId);
                if (item == null)
                    throw new InvalidOperationException(
                        string.Format("The component {0} didnt exist previously. Use RequestJoinNetwork before this.",
                                      sourceComponentId));

                item.IsRunning = status == ComponentStatus.Running;

                ComponentsDataSource.Save(item);
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
                AppComponent component = ComponentsDataSource.GetByComponentId(componentId);

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
                    ComponentsDataSource.Save(component);
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
                AppComponent component = ComponentsDataSource.GetByComponentId(componentId);
                component.ExchangedDefinitions = true;
                component.ComponentExchanges = null;
                ComponentsDataSource.Save(component);

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