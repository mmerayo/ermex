// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.Interfaces;
using ermeX.LayerMessages;
using ermeX.Versioning;
using ermeX.ermeX.Component;

namespace ermeX
{

    public sealed class WorldGate : SoaComponent
    {
        #region Singleton

        private static volatile WorldGate _instance;

        private static readonly object SyncRoot = new object();


        private WorldGate()
        {
        }

        private static WorldGate Instance
        {
            get
            {
                if (_instance == null)
                    lock (SyncRoot)
                        if (_instance == null)
                            _instance = new WorldGate();
                return _instance;
            }
        }

        #endregion

        [Inject]
        private IMessagingManager MessageManager { get; set; }

        [Inject]
        private ISubscriptionsManager SubscriptionsManager { get; set; }

        [Inject]
        private IComponentManager ComponentStarter { get; set; }

        [Inject]
        private IServicePublishingManager ServicePublishingManager { get; set; }

        [Inject]
        private IServicesManager ServicesManager { get; set; }

        public static void Reset()
        {

            Instance.ResetAll();
        }



        public static void ConfigureAndStart(Configuration settings)
        {
            try
            {
                if (settings == null) throw new ArgumentNullException("settings");
                if (Instance.IsStarted)
                    throw new InvalidOperationException("The WorldGate has been already started. Reset it first.");
                RunUpgrades(settings);

                ConfigurationManager.SetSettingsSource(settings.GetSettings<IComponentSettings>());
                IoCManager.Kernel.Inject(Instance);

                Instance.ComponentStarter.Start();

                SubscribeDiscoveredMessageHandlers(settings);

                PublishDiscoveredServices(settings);
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }

        private static void PublishDiscoveredServices(Configuration settings)
        {
            try
            {
                var discoveredServices = settings.GetDiscoveredServices();
                foreach (var svc in discoveredServices)
                {
                    Instance.ServicePublishingManager.PublishService(svc.InterfaceType, svc.ImplementationType);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }

        private static void SubscribeDiscoveredMessageHandlers(Configuration settings)
        {
            try
            {
                var discoveredSubscriptions = settings.GetDiscoveredSubscriptions();
                foreach (var discoveredSubscription in discoveredSubscriptions)
                {
                    Instance.SubscriptionsManager.Subscribe(discoveredSubscription.HandlerType,
                                                            discoveredSubscription.MessageType);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }

        private static void RunUpgrades(Configuration settings)
        {
            try
            {
                //TODO: MUST GO TO CONFIGURATION BUT THEN EVERY ASSET  MUST BE STARTABLE TO FIND THE CONFIGURATION ALWAYS UPDATED
                var dataAccessSettings = settings.GetSettings<IDalSettings>();
                if (dataAccessSettings == null)
                    throw new InvalidOperationException("The settings implementation is not valid");

                //needed to be available for the version
                if (dataAccessSettings.ConfigurationSourceType == DbEngineType.SqliteInMemory)
                {
                    SessionProvider.SetInMemoryDb(dataAccessSettings.ConfigurationConnectionString);
                }


                var versionUpgradeHelper = new VersionUpgradeHelper(); //TODO: from the configuration startup

                versionUpgradeHelper.RunDataSchemaUpgrades(dataAccessSettings.SchemasApplied,
                                                           dataAccessSettings.ConfigurationConnectionString,
                                                           dataAccessSettings.ConfigurationSourceType);
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }


        public static void Publish<TMessage>(TMessage message) where TMessage : class
        {
            try
            {
                var bizMessage = new BizMessage(message);

                Logger.Trace(
                    x =>
                    x("{0} - Created BizMessage for Type: {1} ", bizMessage.MessageId, bizMessage.MessageType.FullName));

                Instance.MessageManager.PublishMessage(bizMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }

        /// <summary>
        ///   Subscribes a type of handler for a messages type
        /// </summary>
        /// <typeparam name="THandler"> the HandlerObject </typeparam>
        /// <param name="handlerType"> </param>
        /// <returns> The created object to handle the messages </returns>
        public static THandler Suscribe<THandler>(Type handlerType)
        {
            try
            {
                return Instance.SubscriptionsManager.Subscribe<THandler>(handlerType);
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }

        /// <summary>
        ///   Subscribes a type of handler for a messages type
        /// </summary>
        /// <typeparam name="THandler"> the HandlerObject </typeparam>
        /// <returns> The created object to handle the messages </returns>
        public static THandler Suscribe<THandler>()
        {
            return Suscribe<THandler>(typeof (THandler));
        }

        /// <summary>
        ///   Subscribes a type of handler for all messages types
        /// </summary>
        /// <param name="handlerType"> </param>
        /// <returns> The created object to handle the messages </returns>
        public static object Suscribe(Type handlerType)
        {
            return Instance.SubscriptionsManager.Subscribe(handlerType);
        }


        public static void RegisterService<TServiceInterface>(Type serviceImplementationType)
            where TServiceInterface : IService
        {
            try
            {
                Instance.ServicePublishingManager.PublishService<TServiceInterface>(serviceImplementationType);
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }

        /// <summary>
        /// Gets one service proxy
        /// </summary>
        /// <typeparam name="TService">Interface of the service</typeparam>
        /// <returns>The service proxy or null if the remote component is offline</returns>
        public static TService GetServiceProxy<TService>()
            where TService : IService
        {
            try
            {
                return Instance.ServicesManager.GetServiceProxy<TService>();
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }


        /// <summary>
        /// Gets the service proxy of one concrete component
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="componentId"></param>
        /// <returns>The service proxy or null if the remote component is offline</returns>
        public static TService GetServiceProxy<TService>(Guid componentId)
            where TService : IService
        {
            try
            {
                return Instance.ServicesManager.GetServiceProxy<TService>(componentId);
            }
            catch (Exception ex)
            {
                Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
                throw ex;
            }
        }
    }
}