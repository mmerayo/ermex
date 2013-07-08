using System.Linq;

using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Services;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnServicesReceivedStepExecutor : IOnServicesReceivedStepExecutor
	{
		private readonly ILogger _logger;
	
		public OnServicesReceivedStepExecutor(IComponentSettings settings)
		{
			_logger = LogManager.GetLogger<OnServicesReceivedStepExecutor>(settings.ComponentId, LogComponent.Handshake);
		}
		
		public void ServicesReceived(IRemoteComponentStateMachineContext context)
		{
			_logger.DebugFormat("ServicesReceived - Component: {0}",context.RemoteComponentId);
		}
		
	}
}