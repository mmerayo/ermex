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
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX;
using ermeX.Bus.Synchronisation.Dialogs.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.DAL.Interfaces.Services;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Exceptions;
using ermeX.Models.Entities;


namespace ermeX.Bus.Synchronisation
{
	//ENCAPSULATES a FULLY CONNECTED MESSY TOPOLOGY
	//TODO: MAKE ARECHITECTURE DECOUPLISHED FROM TOPOLOGY AND TEST OTHER TOPOLOGIES

	internal sealed class DialogsManager : IDialogsManager
	{
		[Inject]
		public DialogsManager(IMessagePublisher publisher, IMessageListener listener,
		                      IComponentSettings settings, IBusSettings busSettings,
		                      ICanReadComponents componentReader,
		                      ICanWriteComponents componentsWritter,
		                      ICanReadServiceDetails serviceDetailsReader,
		                      ICanWriteServiceDetails serviceDetailsWritter,
		                      ICanReadConnectivityDetails connectivityDetailsReader,
		                      ICanWriteConnectivityDetails connectivityDetailsWritter,

		                      IHandshakeService joinNetworkService,
		                      IMessageSuscriptionsService messageSuscriptionsService,
		                      IPublishedServicesDefinitionsService publishedServicesDefinitionsService,
		                      IUpdatePublishedServiceMessageHandler updatePublishedServiceMessageHandler,
		                      IUpdateSuscriptionMessageHandler updateSuscriptionMessageHandler,
		                      IStatusManager statusManager,
		                      IRegisterComponents componentsRegistrator,
		                      ICanReadIncommingMessagesSubscriptions incommingSubscriptionsReader,
		                      ICanUpdateOutgoingMessagesSubscriptions outgoingMessagesSubscriptionsWritter
			)
		{
			if (publisher == null) throw new ArgumentNullException("publisher");
			if (listener == null) throw new ArgumentNullException("listener");
			if (settings == null) throw new ArgumentNullException("settings");
			if (busSettings == null) throw new ArgumentNullException("busSettings");


			if (joinNetworkService == null) throw new ArgumentNullException("joinNetworkService");
			if (messageSuscriptionsService == null) throw new ArgumentNullException("messageSuscriptionsService");
			if (publishedServicesDefinitionsService == null)
				throw new ArgumentNullException("publishedServicesDefinitionsService");
			if (updatePublishedServiceMessageHandler == null)
				throw new ArgumentNullException("updatePublishedServiceMessageHandler");
			if (updateSuscriptionMessageHandler == null)
				throw new ArgumentNullException("updateSuscriptionMessageHandler");
			if (statusManager == null) throw new ArgumentNullException("statusManager");


			Publisher = publisher;
			Listener = listener;
			Settings = settings;
			BusSettings = busSettings;
			ComponentReader = componentReader;
			ComponentsWritter = componentsWritter;
			ServiceDetailsReader = serviceDetailsReader;
			ServiceDetailsWritter = serviceDetailsWritter;
			ConnectivityDetailsReader = connectivityDetailsReader;
			ConnectivityDetailsWritter = connectivityDetailsWritter;

			JoinNetworkService = joinNetworkService;
			MessageSuscriptionsService = messageSuscriptionsService;
			PublishedServicesDefinitionsService = publishedServicesDefinitionsService;
			UpdatePublishedServiceMessageHandler = updatePublishedServiceMessageHandler;
			UpdateSuscriptionMessageHandler = updateSuscriptionMessageHandler;
			StatusManager = statusManager;
			ComponentsRegistrator = componentsRegistrator;
			IncommingSubscriptionsReader = incommingSubscriptionsReader;
			OutgoingMessagesSubscriptionsWritter = outgoingMessagesSubscriptionsWritter;



			//PublishLocalSystemServices();

		}


		private IComponentSettings Settings { get; set; }
		private IBusSettings BusSettings { get; set; }
		private ICanReadComponents ComponentReader { get; set; }
		private ICanWriteComponents ComponentsWritter { get; set; }
		private ICanReadServiceDetails ServiceDetailsReader { get; set; }
		private ICanWriteServiceDetails ServiceDetailsWritter { get; set; }
		private ICanReadConnectivityDetails ConnectivityDetailsReader { get; set; }
		private ICanWriteConnectivityDetails ConnectivityDetailsWritter { get; set; }


		private IHandshakeService JoinNetworkService { get; set; }
		private IMessageSuscriptionsService MessageSuscriptionsService { get; set; }
		private IPublishedServicesDefinitionsService PublishedServicesDefinitionsService { get; set; }
		private IUpdatePublishedServiceMessageHandler UpdatePublishedServiceMessageHandler { get; set; }
		private IUpdateSuscriptionMessageHandler UpdateSuscriptionMessageHandler { get; set; }
		private IStatusManager StatusManager { get; set; }
		private IRegisterComponents ComponentsRegistrator { get; set; }
		private ICanReadIncommingMessagesSubscriptions IncommingSubscriptionsReader { get; set; }
		private ICanUpdateOutgoingMessagesSubscriptions OutgoingMessagesSubscriptionsWritter { get; set; }

		private IMessageListener Listener { get; set; }

		private IMessagePublisher Publisher { get; set; }

		private static readonly ILog Logger = LogManager.GetLogger(typeof(DialogsManager).FullName);

		/// <summary>
		///   Indicates if the component has already joined the rest of components
		/// </summary>
		public bool InNetwork { get; private set; }

		#region IDialogsManager Members

		public void JoinNetwork()
		{
			if (BusSettings.FriendComponent != null)
				JoinNetworkComponent(BusSettings.FriendComponent.ComponentId);


		}

		private void JoinNetworkComponent(Guid componentId)
		{
			Logger.Trace(x => x("HANDSHAKE: Start Joining component {0}", componentId));
			if (BusSettings.FriendComponent == null)
				return;

			//prepare to wait until the exchange was performed
			StatusManager.SyncEvents.CreateEvent(
				ConfigurationManagement.Status.StatusManager.GlobalSync.GlobalEvents.DefinitionsExchanged, componentId);

			var localConnectivity = ConnectivityDetailsReader.Fetch(Settings.ComponentId);

			var joinNetworkServiceProxy = GetHandshakeServiceProxy(componentId);

			var message = new JoinRequestMessage(Settings.ComponentId, localConnectivity.Ip, localConnectivity.Port);
			MyComponentsResponseMessage friendComponents = null;
			try
			{
				friendComponents = joinNetworkServiceProxy.RequestJoinNetwork(message);
				Debug.Assert(friendComponents != null);
			}
			catch (Exception ex)
			{
				Logger.Warn(x => x("Could not join the component {0}. {1}", componentId, ex));
				return;
			}
			AddComponent(friendComponents);

			Logger.Trace(x => x("HANDSHAKE: Finished Joined component {0}", componentId));
		}

		public void ExchangeDefinitions()
		{

			Logger.Info(x => x("HANDSHAKE: Start exchanging definitions Caller {0}", Settings.ComponentId));
			var componentIds = ComponentReader.FetchOtherComponentsNotExchangedDefinitions(true).Select(x => x.ComponentId);

			foreach (var componentId in componentIds)
			{
				//TODO:RAISE THREAD
				//TODO: TRANSACTIONAL
				var component = ComponentReader.Fetch(componentId);
				ExchangeDefinitions(component);

			}
			//TODO:WAIT ALL THREADS
			Logger.Info(x => x("HANDSHAKE: Finished exchanged definitions Caller: {0}", Settings.ComponentId));
		}

		public void ExchangeDefinitions(AppComponent component)
		{

			var handshakeServiceProxy = GetHandshakeServiceProxy(component.ComponentId);
			ComponentStatus componentStatus;
			try
			{
				componentStatus = handshakeServiceProxy.ExchangeComponentStatus(Settings.ComponentId,
				                                                                StatusManager.CurrentStatus);
			}
			catch (ermeXComponentNotAvailableException ex)
			{
				Logger.Info(x => x("HANDSHAKE: DIDNT exchange definitions component {0} is not running. {1}",
				                   component, ex));
				return;
			}
			if (componentStatus == ComponentStatus.Running)
			{
				AppComponent refreshedComponent = ComponentReader.Fetch(component.ComponentId);
				if ((refreshedComponent.ComponentExchanges == null || refreshedComponent.ComponentExchanges == Settings.ComponentId)
				    //TODO: TRANSACTIONAL as locks who performs the exchange
				    && handshakeServiceProxy.CanExchangeDefinitions(Settings.ComponentId))
				{
					component.ComponentExchanges = Settings.ComponentId;
					component.ExchangedDefinitions = false;
					ComponentsWritter.Save(component);

					Logger.Info(x => x("HANDSHAKE: Start exchanging definitions caller {0} component {1}",
					                   Settings.ComponentId, component.ComponentId));
					RequestMessagesSuscriptions(component.ComponentId);
					NotifyMySubscriptions(component.ComponentId);

					RequestPublishedServices(component.ComponentId);
					NotifyMyServices(component.ComponentId);

					component.ExchangedDefinitions = true;
					ComponentsWritter.Save(component);

					handshakeServiceProxy.DefinitionsExchanged(Settings.ComponentId);

					component.ComponentExchanges = null;
					ComponentsWritter.Save(component);

					StatusManager.SyncEvents.SignalEvent(
						ConfigurationManagement.Status.StatusManager.GlobalSync.GlobalEvents.DefinitionsExchanged,
						component.ComponentId);

					Logger.Info(x => x("HANDSHAKE: Finished exchanging definitions caller {0} component {1}",
					                   Settings.ComponentId, component.ComponentId));
				}
			}
			else
			{
				Logger.Info(
					x =>
					x("HANDSHAKE: DIDNT exchange definitions caller {0} component {1} is not running", Settings.ComponentId,
					  component.ComponentId));
			}
		}



		//TODO: ALL THIS HANDLING TO BE ENCAPSULATED AND TRANSACTIONAL OR THREAD SAFE
		private readonly object _notifyStatusLock = new object();

		public void NotifyCurrentStatus()
		{
			var components = ComponentReader.FetchAll()
				.Where(x => x.ComponentId != Settings.ComponentId);


			ComponentStatus componentStatus = StatusManager.CurrentStatus;
			Guid sourceComponentId = Settings.ComponentId;
			lock (_notifyStatusLock) //TODO: RAISE THREAD POOLS to communicate
				foreach (var runningComponent in components)
				{
					IHandshakeService handshakeServiceProxy = GetHandshakeServiceProxy(runningComponent.ComponentId);
					if (handshakeServiceProxy != null)
						try
						{
							ComponentStatus status = handshakeServiceProxy.ExchangeComponentStatus(sourceComponentId, componentStatus);
							if (!runningComponent.IsRunning && status == ComponentStatus.Running)
							{
								runningComponent.IsRunning = true;
								ComponentsWritter.Save(runningComponent);
							}
						}
						catch (ermeXComponentNotAvailableException ex)
						{
							//its logged at one level below
						}
						catch (ApplicationException ex)
						{
							Logger.Warn(x => x("Could not notify the status to {0}. It is probably disconnected. {1}", runningComponent, ex));
						}
				}
		}

		public void UpdateRemoteServiceDefinition(string interfaceName, string methodName)
		{
			Logger.Debug(x => x("Requesting {0}.{1} in all components", interfaceName, methodName));
			var appComponents = ComponentReader.FetchAll().Where(x => x.ComponentId != x.ComponentOwner);

			foreach (var appComponent in appComponents)
				UpdateRemoteServiceDefinition(interfaceName, methodName, appComponent);
			Logger.Debug(x => x("Requested COMPLETED {0}.{1} in all components", interfaceName, methodName));
		}

		public void UpdateRemoteServiceDefinition(string interfaceName, string methodName, AppComponent appComponent)
		{
			if (appComponent == null) throw new ArgumentNullException("appComponent");
			if (string.IsNullOrEmpty(interfaceName)) throw new ArgumentException("interfaceName");
			if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("methodName");

			var proxy = Publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(appComponent.ComponentId);
			if (proxy == null)
				throw new ermeXServiceNotAvailableException(interfaceName, methodName, null, appComponent.ComponentId);

			Logger.Debug(x => x("Requesting {0}.{1} in {2}", interfaceName, methodName, appComponent.ComponentId));
			Guid sourceComponentId = Settings.ComponentId;
			var request = new PublishedServicesRequestMessage(sourceComponentId, interfaceName, methodName);
			PublishedServicesResponseMessage result = proxy.RequestDefinitions(request);

			SaveServiceDefinitions(result);
			Logger.Debug(x => x("Requested FINISHED {0}.{1} in {2}", interfaceName, methodName, appComponent.ComponentId));
		}

		public void EnsureDefinitionsAreExchanged(AppComponent appComponent, int retries = 1)
		{
			const int MaxRetries = 5;
			if (retries == MaxRetries)
				return;
			if (appComponent == null) throw new ArgumentNullException("appComponent");
			int secondsToWait = (int) Math.Pow(1.5, retries);
			StatusManager.SyncEvents.Wait(
				ConfigurationManagement.Status.StatusManager.GlobalSync.GlobalEvents.DefinitionsExchanged,
				appComponent.ComponentId, secondsToWait);

			AppComponent byComponentId = ComponentReader.Fetch(appComponent.ComponentId);
			if (byComponentId == null)
				throw new InvalidOperationException("The component doesnt exist");
			if (!byComponentId.ExchangedDefinitions)
			{
				ExchangeDefinitions(appComponent);
				EnsureDefinitionsAreExchanged(appComponent, retries + 1);
			}
		}

		public void EnsureDefinitionsAreExchanged(IEnumerable<AppComponent> appComponents, int retries = 1)
		{
			const int MaxRetries = 5;

			if (retries == MaxRetries || appComponents == null || !appComponents.Any())
				return;
			int secondsToWait = (int) Math.Pow(2, retries);
			foreach (var appComponent in appComponents)
			{
				StatusManager.SyncEvents.Wait(
					ConfigurationManagement.Status.StatusManager.GlobalSync.GlobalEvents.DefinitionsExchanged,
					appComponent.ComponentId, secondsToWait);
			}
			//re-exchange and also if new components appeared
			IEnumerable<AppComponent> componentsToExchange = ComponentReader.FetchOtherComponentsNotExchangedDefinitions(true);

			foreach (var appComponent in componentsToExchange)
			{
				ExchangeDefinitions(appComponent);
			}

			EnsureDefinitionsAreExchanged(componentsToExchange, retries + 1);
		}

		public void Start() //TODO: METHDOS TO NOT TO WORK IF NOT STARTED
		{
			//Creates this service details as is an special case
			if (BusSettings.FriendComponent != null)
				ComponentsRegistrator.CreateRemoteComponent(BusSettings.FriendComponent.ComponentId,
				                                            BusSettings.FriendComponent.Endpoint.Address.ToString(),
				                                            BusSettings.FriendComponent.Endpoint.Port);
		}


		public void Suscribe(Guid subscriptionHandlerId)
		{
			var appComponents = ComponentReader.FetchOtherComponents().Select(x => x.ComponentId); //get other components
			var subscription = IncommingSubscriptionsReader.GetByHandlerId(subscriptionHandlerId);
			//get the phisical subscription
			foreach (var appComponent in appComponents)
			{
				var proxy = Publisher.GetServiceProxy<IMessageSuscriptionsService>(appComponent);
				proxy.AddSuscription(subscription);
			}
		}

		public void NotifyService<TService>(Type serviceImplementationType) where TService : IService
		{
			NotifyService(typeof (TService), serviceImplementationType);
		}

		public void NotifyService(Type serviceInterface, Type serviceImplementation)
		{
			if (serviceInterface == null) throw new ArgumentNullException("serviceInterface");
			if (serviceImplementation == null) throw new ArgumentNullException("serviceImplementation");
			var appComponents = ComponentReader.FetchOtherComponents();

			//ISSUE-281: From reader
			var serviceOperations =
				ServiceDetailsReader.GetByInterfaceType(serviceInterface.FullName).Where(x => x.Publisher == Settings.ComponentId).
					ToList();

			foreach (var appComponent in appComponents)
			{
				var proxy = Publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(appComponent.ComponentId);
				foreach (var serviceOperation in serviceOperations)
				{
					proxy.AddService(serviceOperation);
				}
			}
		}


		#endregion


		public IHandshakeService GetHandshakeServiceProxy(Guid componentId)
		{
			return Publisher.GetServiceProxy<IHandshakeService>(componentId);
		}

		private readonly object _addComponentLocker = new object(); //TODO: REPLACE BY TRANSACTIONS

		private void AddComponent(MyComponentsResponseMessage componentsMessage)
		{
			var pendingJoinRequests = new List<Guid>();

			lock (_addComponentLocker)
			{
				foreach (var componentData in componentsMessage.Components)
				{
					//THIS MIGHT NEED TO BE REMOVED AS REDUNDANT OR BECAUSE IT DOESNT LET THE UPDATE ONCE THE COMPONENT WAS REGISTERED, CHECK THIS
					if (ComponentReader.Fetch(componentData.Item1.ComponentId) != null)
						continue;

					//TODO: ISSUE-281: move logic
					var isNew = ComponentsRegistrator.CreateRemoteComponent(componentData.Item2.ServerId,
																		   componentData.Item2.Ip, 
																		   componentData.Item2.Port);
					if (isNew)
						pendingJoinRequests.Add(componentData.Item1.ComponentId);

				}

			}
			foreach (var compId in pendingJoinRequests)
			{
				JoinNetworkComponent(compId);
			}
		}


		private void RequestPublishedServices(Guid componentId)
		{
			var proxy = Publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(componentId);

			var request = new PublishedServicesRequestMessage(Settings.ComponentId);
			var remoteDefinitions = proxy.RequestDefinitions(request);

			SaveServiceDefinitions(remoteDefinitions);
		}

		private void SaveServiceDefinitions(PublishedServicesResponseMessage remoteDefinitions)
		{
			if (remoteDefinitions.LocalServiceDefinitions != null)
				foreach (var svc in remoteDefinitions.LocalServiceDefinitions)
					ServiceDetailsWritter.ImportFromOtherComponent(svc);
		}

		private void RequestMessagesSuscriptions(Guid componentId)
		{
			var proxy = Publisher.GetServiceProxy<IMessageSuscriptionsService>(componentId);

			var request = new MessageSuscriptionsRequestMessage(Settings.ComponentId);
			var remoteSuscriptions = proxy.RequestSuscriptions(request);

			//remote incomming is local outgoing
			if (remoteSuscriptions.MyIncomingSuscriptions != null)
					OutgoingMessagesSubscriptionsWritter.ImportFromOtherComponent(remoteSuscriptions.MyIncomingSuscriptions);

			//remote outgoing is local outgoing but local subscriptions
			if (remoteSuscriptions.MyOutgoingSuscriptions != null)
			{
				var outgoingMessageSuscriptions = remoteSuscriptions.MyOutgoingSuscriptions.Where(x => x.Component != Settings.ComponentId);
				OutgoingMessagesSubscriptionsWritter.ImportFromOtherComponent(outgoingMessageSuscriptions);
			}
		}

		private void NotifyMySubscriptions(Guid componentId)
		{
			var myIncomingSubscriptions = IncommingSubscriptionsReader.FetchAll();
			//get my subscriptions that are not from componentId

			var proxy = Publisher.GetServiceProxy<IMessageSuscriptionsService>(componentId);
			if (myIncomingSubscriptions != null && myIncomingSubscriptions.Any())
				proxy.AddSuscriptions(myIncomingSubscriptions);
		}

		private void NotifyMyServices(Guid componentId)
		{
			var myServices = ServiceDetailsReader.GetLocalCustomServices();

			var proxy = Publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(componentId);
			if (myServices != null && myServices.Any())
				foreach (var svc in myServices)
				{
					proxy.AddService(svc);
					//TODO: MUST WORK WITH COLLECTIONS AND IS NOT WORKING AT THE MOMENT CHECK BYTES, NULL FUIELDS ETC
				}
		}



	}
}