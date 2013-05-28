namespace ermeX.ComponentServices.Interfaces.RemoteComponent.Commands
{
	internal interface IOnCreatingStepExecutor
	{
		void Create(IRemoteComponentStateMachineContext context);
	}
}