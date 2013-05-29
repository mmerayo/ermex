using System;
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.ComponentServices.Interfaces.LocalComponent;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Services;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.LayerMessages;
using ermeX.Models.Entities;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponent : ILocalComponent
	{
		private readonly ICanReadServiceDetails _serviceDetailsReader;

		//TODO: THE FOLLOWING 3 HAVE SIMILAR COMMITMENTS, PUT THEM UNDER SAME COMPONENT INTERFACE
		private readonly IMessagePublisher _publisher;
		private readonly IMessagingManager _messagingManager;
		private readonly ISubscriptionsManager _subscriptionsManager;

		//TODO: FOLLOWING 2 TYPES UNDER SAME INTERFACE
		private readonly IServicePublishingManager _servicePublishingManager;
		private readonly IServicesManager _servicesManager;

		private readonly ICanReadIncommingMessagesSubscriptions _incommingMessagesSubscriptionsReader;
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponent>();
		private readonly LocalComponentStateMachine _stateMachine;

		[Inject]
		public LocalComponent(
			LocalComponentStateMachine stateMachine,
			ICanReadServiceDetails serviceDetailsReader,
			IMessagePublisher publisher,
			ICanReadIncommingMessagesSubscriptions incommingMessagesSubscriptionsReader,
			IMessagingManager messagingManager,
			ISubscriptionsManager subscriptionsManager,
			IServicePublishingManager servicePublishingManager,
			IServicesManager servicesManager
			)
		{
			_serviceDetailsReader = serviceDetailsReader;
			_publisher = publisher;
			_incommingMessagesSubscriptionsReader = incommingMessagesSubscriptionsReader;
			_messagingManager = messagingManager;
			_subscriptionsManager = subscriptionsManager;
			_servicePublishingManager = servicePublishingManager;
			_servicesManager = servicesManager;

			_stateMachine = stateMachine;
		}

		public void Start()
		{
			Logger.Debug("Start");
			_stateMachine.Start();
		}

		//TODO: THE FOLLOWING 2 METHODS SMELL HERE

		public void PublishMyServices(Guid componentId)
		{
			Logger.DebugFormat("PublishMyServices -  To Component:{0}",componentId);
			//TODO: ABSTRACT TO ANOTHER SERVICE
			var myServices = _serviceDetailsReader.GetLocalCustomServices();

			var proxy = _publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(componentId);
			var serviceDetailses = myServices as ServiceDetails[] ?? myServices.ToArray();
			if (myServices != null && serviceDetailses.Any())
				foreach (var svc in serviceDetailses)
				{
					proxy.AddService(svc);
					//TODO: MUST WORK WITH COLLECTIONS AND IS NOT WORKING AT THE MOMENT CHECK BYTES, NULL FUIELDS ETC
				}
		}

		public void PublishMySubscriptions(Guid componentId)
		{
			Logger.DebugFormat("PublishMySubscriptions -  To Component:{0}", componentId);
			//TODO: ABSTRACT TO ANOTHER SERVICE

			var myIncomingSubscriptions = _incommingMessagesSubscriptionsReader.FetchAll();
			//get my subscriptions that are not from componentId

			var proxy = _publisher.GetServiceProxy<IMessageSuscriptionsService>(componentId);
			if (myIncomingSubscriptions != null && myIncomingSubscriptions.Any())
				proxy.AddSuscriptions(myIncomingSubscriptions);
		}

		public void Stop()
		{
			_stateMachine.Stop();
		}

		public bool IsRunning()
		{
			return _stateMachine.IsRunning();
		}

		public void PublishMessage(BizMessage bizMessage)
		{
			_messagingManager.PublishMessage(bizMessage);
		}

		public THandler Subscribe<THandler>(Type handlerType)
		{
			return _subscriptionsManager.Subscribe<THandler>(handlerType);
		}

		public object Subscribe(Type handlerType)
		{
			return _subscriptionsManager.Subscribe(handlerType);
		}

		public void PublishService<TServiceInterface>(Type serviceImplementationType) where TServiceInterface : IService
		{
			_servicePublishingManager.PublishService<TServiceInterface>(serviceImplementationType);
		}

		public TService GetServiceProxy<TService>() where TService : IService
		{
			return _servicesManager.GetServiceProxy<TService>();
		}

		public TService GetServiceProxy<TService>(Guid componentId) where TService : IService
		{
			return _servicesManager.GetServiceProxy<TService>(componentId);
		}
	}
}