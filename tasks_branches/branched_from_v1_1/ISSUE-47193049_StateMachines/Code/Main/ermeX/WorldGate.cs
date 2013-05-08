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
using ermeX.Biz;
using ermeX.Biz.Interfaces;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Providers;
using ermeX.LayerMessages;
using ermeX.Versioning;
using ermeX.ermeX.Component;


namespace ermeX
{
	/// <summary>
	/// Interface that exposes all the functionallity of ermeX
	/// </summary>
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

		[Inject]
		private IVersionUpgradeHelper VersionUpgradeHelper { get; set; }

		/// <summary>
		/// Stops & Resets the component
		/// </summary>
		/// <remarks>Reconfigure after usage using method ConfigureAndStart</remarks>
		public static void Reset()
		{
			Instance.ResetAll();
		}

		/// <summary>
		/// Initialises the component with the settings in the app.config file
		/// </summary>
		public static void ConfigureAndStart()
		{
			var settings = ConfigurationManager.GetSettingsFromConfig();
			ConfigureAndStart(settings);
		}

		/// <summary>
		/// Configures the component with the settings provided and starts it joining it to the ermeX Network
		/// </summary>
		/// <param name="settings">The component configuration</param>        
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
		public static void ConfigureAndStart(Configurer settings)
		{
			try
			{
				if (settings == null) throw new ArgumentNullException("settings");
				if (Instance.IsStarted)
					throw new InvalidOperationException("The WorldGate has been already started. Reset it first.");

				

				ConfigurationManager.SetSettingsSource(settings.GetSettings<IComponentSettings>());
				IoCManager.Kernel.Inject(Instance);
				RunUpgrades(settings);

				Instance.ComponentStarter.Start();

				SubscribeDiscoveredMessageHandlers(settings);

				PublishDiscoveredServices(settings);
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
				throw;
			}
		}

		private static void PublishDiscoveredServices(Configurer settings)
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
				throw;
			}
		}

		private static void SubscribeDiscoveredMessageHandlers(Configurer settings)
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

		private static void RunUpgrades(Configurer settings)
		{
			try
			{
				//TODO: MUST GO TO CONFIGURATION BUT THEN EVERY ASSET  MUST BE STARTABLE TO FIND THE CONFIGURATION ALWAYS UPDATED
				var dataAccessSettings = settings.GetSettings<IDalSettings>();
				if (dataAccessSettings == null)
					throw new InvalidOperationException("The settings implementation is not valid");
				//TODO: DEcouple this class and ensure the versionupgrade is injected
				//TODO: ISSUE-281 -->MAKE THIS internal
				if (dataAccessSettings.ConfigurationSourceType == DbEngineType.SqliteInMemory)
				{
					SessionProvider.SetInMemoryDb(dataAccessSettings.ConfigurationConnectionString);
				}
				
				Instance.VersionUpgradeHelper.RunDataSchemaUpgrades(dataAccessSettings.SchemasApplied,
				                                           dataAccessSettings.ConfigurationConnectionString,
				                                           dataAccessSettings.ConfigurationSourceType);
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("Unhandled ermeX exception: {0}", ex));
				throw ex;
			}
		}

		/// <summary>
		/// Publishes one message 
		/// </summary>
		/// <typeparam name="TMessage">Message type. it must be a reference type</typeparam>
		/// <param name="message">The message to be published</param>        
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
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
		///  Subscribes a type of handler for a messages type
		/// </summary>
		/// <typeparam name="THandler"> the Handler type to be returned </typeparam>
		/// <param name="handlerType"> the subscriber implementation type </param>
		/// <returns> The created object to handle the messages </returns>
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
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
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
		public static THandler Suscribe<THandler>()
		{
			return Suscribe<THandler>(typeof (THandler));
		}

		/// <summary>
		///   Subscribes a type of message handler
		/// </summary>
		/// <param name="handlerType">The type of the message handler</param>
		/// <returns> The created object to handle the messages </returns>
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
		public static object Suscribe(Type handlerType)
		{
			return Instance.SubscriptionsManager.Subscribe(handlerType);
		}

		/// <summary>
		/// Registers a service exposed by the component to the ermeX network
		/// </summary>
		/// <typeparam name="TServiceInterface">Service interface. <remarks>Implements IService</remarks></typeparam>
		/// <param name="serviceImplementationType">Service Implementation<remarks>Implements TServiceInterface</remarks></param>
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
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
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
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
		/// <typeparam name="TService">Interface of the service</typeparam>
		/// <param name="componentId">The componentId that is performing the service requests. To use when there could be several exposing the same service</param>
		/// <returns>The service proxy or null if the remote component is offline</returns>
		/// <remarks>any issue or question? please report it here "http://code.google.com/p/ermex/issues/entry" </remarks>
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