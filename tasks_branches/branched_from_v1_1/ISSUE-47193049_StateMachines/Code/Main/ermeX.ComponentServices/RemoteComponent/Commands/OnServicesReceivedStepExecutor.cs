using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.DAL.Interfaces.Services;
using ermeX.Models.Entities;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnServicesReceivedStepExecutor : IOnServicesReceivedStepExecutor
	{
		private static readonly ILog Logger = LogManager.GetLogger<OnServicesReceivedStepExecutor>();
	

		
		public void ServicesReceived(IRemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("ServicesReceived - Component: {0}",context.RemoteComponentId);
		}
		
	}
}