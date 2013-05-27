namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal interface IOnRunningStepExecutor
	{
		void OnRunning(RemoteComponentStateMachineContext context);
	}
}