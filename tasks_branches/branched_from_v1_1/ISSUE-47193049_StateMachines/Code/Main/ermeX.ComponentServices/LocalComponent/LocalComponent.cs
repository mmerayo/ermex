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
using ermeX.Models.Entities;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponent : ILocalComponent
	{
		private readonly ICanReadServiceDetails _serviceDetailsReader;
		private readonly IMessagePublisher _publisher;
		private readonly ICanReadIncommingMessagesSubscriptions _incommingMessagesSubscriptionsReader;
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponent>();
		private readonly LocalComponentStateMachine _stateMachine;

		[Inject]
		public LocalComponent(
			LocalComponentStateMachine stateMachine,
			ICanReadServiceDetails serviceDetailsReader,
			IMessagePublisher publisher,
			ICanReadIncommingMessagesSubscriptions incommingMessagesSubscriptionsReader
			)
		{
			_serviceDetailsReader = serviceDetailsReader;
			_publisher = publisher;
			_incommingMessagesSubscriptionsReader = incommingMessagesSubscriptionsReader;

			_stateMachine = stateMachine;
		}

		public void Start()
		{
			Logger.Debug("Start");
			_stateMachine.Start();
		}

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

		public bool IsStarted()
		{
			return _stateMachine.IsRunning();
		}
	}
}