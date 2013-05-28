namespace ermeX.ComponentServices.Interfaces.RemoteComponent.Commands
{
	internal interface IOnRunningStepExecutor
	{
		void OnRunning(IRemoteComponentStateMachineContext context);
	}
}