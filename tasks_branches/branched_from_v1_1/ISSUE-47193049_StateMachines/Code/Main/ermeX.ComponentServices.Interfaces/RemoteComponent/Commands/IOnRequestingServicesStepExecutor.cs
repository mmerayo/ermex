
namespace ermeX.ComponentServices.Interfaces.RemoteComponent.Commands
{
	internal interface IOnRequestingServicesStepExecutor
	{
		void Request(IRemoteComponentStateMachineContext context);
	}
}