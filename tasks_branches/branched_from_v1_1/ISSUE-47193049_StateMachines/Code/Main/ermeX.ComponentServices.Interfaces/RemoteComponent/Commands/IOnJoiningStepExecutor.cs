namespace ermeX.ComponentServices.Interfaces.RemoteComponent.Commands
{
	internal interface IOnJoiningStepExecutor
	{
		void Join(IRemoteComponentStateMachineContext context);
	}
}