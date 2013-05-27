using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.DAL.Interfaces.Services;
using ermeX.Models.Entities;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnServicesReceivedStepExecutor : IOnServicesReceivedStepExecutor
	{
		private static readonly ILog Logger = LogManager.GetLogger<OnServicesReceivedStepExecutor>();
	

		
		public void ServicesReceived(RemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("ServicesReceived - Component: {0}",context.ComponentId);
		}
		
	}
}