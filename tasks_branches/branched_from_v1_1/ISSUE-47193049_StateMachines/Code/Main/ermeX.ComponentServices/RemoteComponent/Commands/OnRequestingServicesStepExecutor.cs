
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Services;
using ermeX.Logging;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal sealed class OnRequestingServicesStepExecutor : IOnRequestingServicesStepExecutor
	{
		private readonly IMessagePublisher _publisher;
		private readonly IComponentSettings _settings;
		private readonly ICanWriteServiceDetails _serviceDetailsWritter;
		private readonly ILogger _logger;

		[Inject]
		public OnRequestingServicesStepExecutor(
			IMessagePublisher publisher,
			IComponentSettings settings,
			ICanWriteServiceDetails serviceDetailsWritter)
		{
			_logger = LogManager.GetLogger<OnRequestingServicesStepExecutor>(settings.ComponentId,LogComponent.Handshake);
			_publisher = publisher;
			_settings = settings;
			_serviceDetailsWritter = serviceDetailsWritter;
		}

		public void Request(IRemoteComponentStateMachineContext context)
		{
			_logger.DebugFormat("Request- Component:", context.RemoteComponentId);

			var proxy = _publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(context.RemoteComponentId);

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