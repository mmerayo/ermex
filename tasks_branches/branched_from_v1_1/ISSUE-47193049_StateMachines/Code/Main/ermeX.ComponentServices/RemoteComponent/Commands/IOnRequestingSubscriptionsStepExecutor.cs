namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal interface IOnRequestingSubscriptionsStepExecutor
	{
		void Request(IRemoteComponentStateMachineContext context);
	}
}