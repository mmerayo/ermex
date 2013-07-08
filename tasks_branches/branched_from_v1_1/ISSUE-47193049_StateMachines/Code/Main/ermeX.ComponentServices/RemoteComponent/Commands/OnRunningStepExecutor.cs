
using System;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Logging;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnRunningStepExecutor : IOnRunningStepExecutor
	{
		private readonly IMessagePublisher _publisher;
		private readonly ILogger _logger ;

		public OnRunningStepExecutor(IMessagePublisher publisher,IComponentSettings settings)
		{
			_logger = LogManager.GetLogger<OnRunningStepExecutor>(settings.ComponentId, LogComponent.Handshake);
			_publisher = publisher;
		}

		public void OnRunning(IRemoteComponentStateMachineContext context)
		{
			_logger.DebugFormat("OnRunning- RemoteComponent:{1}" ,context.RemoteComponentId);
			if (!context.RemoteExecutesJoin)
			{
				var handshakeService = _publisher.GetServiceProxy<IHandshakeService>(context.RemoteComponentId);
				handshakeService.HandshakeCompleted(context.RemoteComponentId);
			}

		}
	}
}