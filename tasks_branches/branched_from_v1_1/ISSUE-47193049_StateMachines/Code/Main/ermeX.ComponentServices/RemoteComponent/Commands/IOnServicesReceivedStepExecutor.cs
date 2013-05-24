namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal interface IOnServicesReceivedStepExecutor
	{
		void ServicesReceived(RemoteComponentStateMachineContext context);
	}
}