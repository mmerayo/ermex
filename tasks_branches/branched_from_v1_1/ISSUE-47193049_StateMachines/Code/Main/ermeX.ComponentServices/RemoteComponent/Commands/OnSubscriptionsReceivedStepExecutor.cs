using Common.Logging;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnSubscriptionsReceivedStepExecutor : IOnSubscriptionsReceivedStepExecutor
	{
		private static readonly ILog Logger = LogManager.GetLogger<OnSubscriptionsReceivedStepExecutor>();
		public void SubscriptionsReceived(IRemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("SubscriptionsReceived- Component:{0}", context.ComponentId);

			//TODO: PARALLEL TASK
			ComponentManager.Default.LocalComponent.PublishMyServices(context.ComponentId);
			ComponentManager.Default.LocalComponent.PublishMySubscriptions(context.ComponentId);
		}
	}
}