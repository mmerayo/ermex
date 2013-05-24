namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal interface IOnCreatingStepExecutor
	{
		void Create(IRemoteComponentStateMachineContext context);
	}
}