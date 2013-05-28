namespace ermeX.ComponentServices.Interfaces.RemoteComponent.Commands
{
	internal interface IOnRequestingSubscriptionsStepExecutor
	{
		void Request(IRemoteComponentStateMachineContext context);
	}
}