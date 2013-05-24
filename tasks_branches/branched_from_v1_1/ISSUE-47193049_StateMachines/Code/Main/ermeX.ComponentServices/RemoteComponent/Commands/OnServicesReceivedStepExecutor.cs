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
		private readonly ICanReadServiceDetails _serviceDetailsReader;
		private readonly IMessagePublisher _publisher;

		[Inject]
		public OnServicesReceivedStepExecutor(ICanReadServiceDetails serviceDetailsReader,
			IMessagePublisher publisher)
		{
			_serviceDetailsReader = serviceDetailsReader;
			_publisher = publisher;
		}

		public void ServicesReceived(RemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("ServicesReceived - Component: {0}",context.ComponentId);
			
			//TODO: NOW NOTIFIES LOCAL SERVICES, THIS SHOULD BE DONE IN a PARALLEL statemachine
			
			var myServices = _serviceDetailsReader.GetLocalCustomServices();

			var proxy = _publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(context.ComponentId);
			var serviceDetailses = myServices as ServiceDetails[] ?? myServices.ToArray();
			if (myServices != null && serviceDetailses.Any())
				foreach (var svc in serviceDetailses)
				{
					proxy.AddService(svc);
					//TODO: MUST WORK WITH COLLECTIONS AND IS NOT WORKING AT THE MOMENT CHECK BYTES, NULL FUIELDS ETC
				}
		}
		
	}
}