namespace ermeX.ComponentServices.Interfaces.RemoteComponent.Commands
{
	internal interface IOnSubscriptionsReceivedStepExecutor
	{
		void SubscriptionsReceived(IRemoteComponentStateMachineContext context);
	}
}