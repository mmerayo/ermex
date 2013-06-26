using System;
using System.Linq;

using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.ComponentServices.Interfaces.LocalComponent;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Services;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.LayerMessages;
using ermeX.Models.Entities;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponent : ILocalComponent
	{
		private ICanReadServiceDetails _serviceDetailsReader;

		//TODO: THE FOLLOWING 3 HAVE SIMILAR COMMITMENTS, PUT THEM UNDER SAME COMPONENT INTERFACE
		private  IMessagePublisher _publisher;
		private  IMessagingManager _messagingManager;
		private  ISubscriptionsManager _subscriptionsManager;

		//TODO: FOLLOWING 2 TYPES UNDER SAME INTERFACE
		private  IServicePublishingManager _servicePublishingManager;
		private  IServicesManager _servicesManager;

		private  ICanReadIncommingMessagesSubscriptions _incommingMessagesSubscriptionsReader;
		private static readonly ILogger Logger = LogManager.GetLogger<LocalComponent>();
		private readonly LocalComponentStateMachine _stateMachine;

		[Inject]
		public LocalComponent(LocalComponentStateMachine stateMachine)
		{
			_stateMachine = stateMachine;
		}

		public void Start()
		{
			Logger.Debug("Start");

			RefreshInjections();

			_stateMachine.Start();
		}

		//needed before the start oif the cicle as the container might have been reset
		private void RefreshInjections()
		{
			_serviceDetailsReader = IoCManager.Kernel.Get<ICanReadServiceDetails>();
			_publisher = IoCManager.Kernel.Get < IMessagePublisher>();
			_incommingMessagesSubscriptionsReader = IoCManager.Kernel.Get<ICanReadIncommingMessagesSubscriptions>();
			_messagingManager = IoCManager.Kernel.Get<IMessagingManager>();
			_subscriptionsManager = IoCManager.Kernel.Get<ISubscriptionsManager>();
			_servicePublishingManager = IoCManager.Kernel.Get<IServicePublishingManager>();
			_servicesManager = IoCManager.Kernel.Get<IServicesManager>();
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


		public bool IsStopped()
		{
			return _stateMachine.IsStopped();
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