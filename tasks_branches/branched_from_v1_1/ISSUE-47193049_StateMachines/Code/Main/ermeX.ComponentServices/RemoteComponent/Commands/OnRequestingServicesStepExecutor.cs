using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Services;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal class OnRequestingServicesStepExecutor : IOnRequestingServicesStepExecutor
	{
		private readonly IMessagePublisher _publisher;
		private readonly IComponentSettings _settings;
		private readonly ICanWriteServiceDetails _serviceDetailsWritter;
		private static readonly ILog Logger = LogManager.GetLogger<OnRequestingServicesStepExecutor>();

		[Inject]
		public OnRequestingServicesStepExecutor(IMessagePublisher publisher,
			IComponentSettings settings,
			ICanWriteServiceDetails serviceDetailsWritter)
		{
			_publisher = publisher;
			_settings = settings;
			_serviceDetailsWritter = serviceDetailsWritter;
		}

		public void Request(IRemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("Request- Component:", context.ComponentId);

			var proxy = _publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(context.ComponentId);

			var request = new PublishedServicesRequestMessage(_settings.ComponentId);
			var remoteDefinitions = proxy.RequestDefinitions(request);

			SaveServiceDefinitions(remoteDefinitions);

			//TODO: MOVE WHEN ASYNCHRONOUS
			context.StateMachine.ServicesReceived();
		}

		private void SaveServiceDefinitions(PublishedServicesResponseMessage remoteDefinitions)
		{
			if (remoteDefinitions.LocalServiceDefinitions != null)
				foreach (var svc in remoteDefinitions.LocalServiceDefinitions)
					_serviceDetailsWritter.ImportFromOtherComponent(svc);
		}
	
	}
}