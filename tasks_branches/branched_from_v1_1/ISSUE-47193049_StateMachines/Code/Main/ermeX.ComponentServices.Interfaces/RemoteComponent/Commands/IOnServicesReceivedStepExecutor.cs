namespace ermeX.ComponentServices.Interfaces.RemoteComponent.Commands
{
	internal interface IOnServicesReceivedStepExecutor
	{
		void ServicesReceived(IRemoteComponentStateMachineContext context);
	}
}