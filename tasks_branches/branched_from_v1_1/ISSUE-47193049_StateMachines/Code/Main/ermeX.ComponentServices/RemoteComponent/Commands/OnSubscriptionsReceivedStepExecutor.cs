
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnSubscriptionsReceivedStepExecutor : IOnSubscriptionsReceivedStepExecutor
	{
		private static readonly ILogger Logger = LogManager.GetLogger<OnSubscriptionsReceivedStepExecutor>();
		public void SubscriptionsReceived(IRemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("SubscriptionsReceived- Component:{0}", context.RemoteComponentId);

			//TODO: PARALLEL TASK
			ComponentManager.Default.LocalComponent.PublishMyServices(context.RemoteComponentId);
			ComponentManager.Default.LocalComponent.PublishMySubscriptions(context.RemoteComponentId);
		}
	}
}