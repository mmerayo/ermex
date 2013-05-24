using Ninject;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	internal interface IOnRequestingServicesStepExecutor
	{
		void Request(IRemoteComponentStateMachineContext context);
	}
}