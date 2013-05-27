using Common.Logging;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnRunningStepExecutor : IOnRunningStepExecutor
	{
		private static readonly ILog Logger = LogManager.GetLogger<OnRunningStepExecutor>();
		public void OnRunning(RemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("OnRunning- Component:{0}" ,context.ComponentId);
			//TODO: USE PARALLELTASK
			ComponentManager.Default.LocalComponent.PublishMyServices(context.ComponentId);
			ComponentManager.Default.LocalComponent.PublishMySubscriptions(context.ComponentId);
		}
	}
}