﻿using Common.Logging;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnRunningStepExecutor : IOnRunningStepExecutor
	{
		private static readonly ILog Logger = LogManager.GetLogger<OnRunningStepExecutor>();
		public void OnRunning(IRemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("OnRunning- Component:{0}" ,context.ComponentId);

			//TODO: PARALLEL TASK
			ComponentManager.Default.LocalComponent.PublishMyServices(context.ComponentId);
			ComponentManager.Default.LocalComponent.PublishMySubscriptions(context.ComponentId);
		}
	}
}