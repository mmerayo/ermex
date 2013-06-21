using Common.Logging;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnRunningStepExecutor : IOnRunningStepExecutor
	{
		private readonly IMessagePublisher _publisher;
		private static readonly ILog Logger = LogManager.GetLogger<OnRunningStepExecutor>();

		public OnRunningStepExecutor(IMessagePublisher publisher)
		{
			_publisher = publisher;
		}

		public void OnRunning(IRemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("OnRunning- Component:{0}" ,context.RemoteComponentId);
			if (context.RemoteExecutesJoin)
			{
				var handshakeService = _publisher.GetServiceProxy<IHandshakeService>(context.RemoteComponentId);
				handshakeService.HandshakeCompleted(context.RemoteComponentId);
			}

		}
	}
}