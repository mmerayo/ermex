// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;

namespace ermeX.Biz
{
    //TODO: THIS AND DIALOGSMANAGER SHOULD ENCAPSULATE THE CONNECTION FUNCTIONALLITY IN A PACKAGE AND BE EXTENSIBLE HIDING THE PROTOCOL
    internal sealed class ComponentManager : IComponentManager
    {
        [Inject]
        public ComponentManager(IBizSettings settings, IMessagePublisher publisher, IMessageListener listener,
                                IDialogsManager dialogsManager, IAppComponentDataSource appComponentDataSource,
            IStatusManager statusManager, IAutoRegistration register)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (dialogsManager == null) throw new ArgumentNullException("dialogsManager");
            if (appComponentDataSource == null) throw new ArgumentNullException("appComponentDataSource");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            if (register == null) throw new ArgumentNullException("register");
            Settings = settings;
            Publisher = publisher;
            Listener = listener;
            DialogsManager = dialogsManager;
            AppComponentDataSource = appComponentDataSource;
            StatusManager = statusManager;
            Register = register;

            Register.CreateLocalSetOfData(Settings.Port);
        }

        private IBizSettings Settings { get; set; }
        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private IDialogsManager DialogsManager { get; set; }
        private IAppComponentDataSource AppComponentDataSource { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
        private IStatusManager _statusManager;
        private IStatusManager StatusManager
        {
            get { return _statusManager; }
            set
            {
                if(_statusManager!=null)
                    _statusManager.StatusChanged -= _statusManager_StatusChanged;
                
                _statusManager = value;
                _statusManager.StatusChanged += _statusManager_StatusChanged;
            }
        }

        private IAutoRegistration Register { get; set; }

        void _statusManager_StatusChanged(object sender, ComponentStatus newStatus)
        {
            AppComponentDataSource.SetComponentRunningStatus(Settings.ComponentId, newStatus, true); //TODO: remove second param when fixed, the exchanged definitions are set to false for all components at some point

            switch (newStatus)
            {
                case ComponentStatus.Stopped:
                    break;
                case ComponentStatus.Starting:
                    break;
                case ComponentStatus.Running:
                    DialogsManager.NotifyCurrentStatus();

                    DialogsManager.ExchangeDefinitions();
                    Logger.Trace(x=>x("Component: {0} exchanged definitions", Settings.ComponentId));
                    break;
                case ComponentStatus.Stopping:
                    try
                    {
                        DialogsManager.NotifyCurrentStatus();
                    }catch(Exception ex)
                    {
                        Logger.Warn(x=>x("Could not notify stopping status to all components. Component: {0} Exception: {1}" , Settings.ComponentId,ex));
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
                StatusManager.CurrentStatus = ComponentStatus.Starting;
                Logger.Trace(x=>x("Component: {0} is STARTING", Settings.ComponentId));                

                Listener.Start();
                Publisher.Start();
                
                DialogsManager.JoinNetwork();                
                StatusManager.CurrentStatus = ComponentStatus.Running; //here so it accepts requests, see status changed handler above
                Logger.Trace(x=>x("Component: {0} is RUNNING",Settings.ComponentId));


                IEnumerable<AppComponent> appComponents =
                    AppComponentDataSource.GetOtherComponentsWhereDefinitionsNotExchanged();
                
                DialogsManager.EnsureDefinitionsAreExchanged(appComponents);

            }
            catch (Exception ex)
            {

                Logger.Error(x=>x( "Component Exception on Start", ex));
                throw;
            }
        }

       

        
        #endregion

       
    }
}