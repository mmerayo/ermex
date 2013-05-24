namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal interface IOnJoiningStepExecutor
	{
		void Join(RemoteComponentStateMachineContext context);
	}
}