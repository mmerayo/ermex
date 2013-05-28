using System;
using System.Net;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnJoiningStepExecutor : IOnJoiningStepExecutor
	{
		private static readonly ILog Logger = LogManager.GetLogger<OnJoiningStepExecutor>();

		private readonly IMessagePublisher _publisher;

		[Inject]
		public OnJoiningStepExecutor(IMessagePublisher publisher)
		{
			_publisher = publisher;
		}

		public void Join(RemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("Join- Component:", context.ComponentId);

			var handshakeService = _publisher.GetServiceProxy<IHandshakeService>(context.ComponentId);
			var message = new JoinRequestMessage(context.ComponentId, context.IpAddress.ToString(), context.Port);
			MyComponentsResponseMessage response = null;
			try
			{
				response = handshakeService.RequestJoinNetwork(message);
			}
			catch (Exception ex)
			{
				Logger.Warn(x => x("Join - Could not join the component {0}. Reason {1}", context.ComponentId, ex));
				throw;
			}
			AddComponentsFromResponse(response);
		}

		private static void AddComponentsFromResponse(MyComponentsResponseMessage response)
		{
			foreach (var componentData in response.Components)
			{
				IPAddress ipAddress;
				if (!IPAddress.TryParse(componentData.Item2.Ip, out ipAddress))
				{
					Logger.WarnFormat("Join - Component not recognized. Could not parse the IP: {0} of the Component: {1}",
					                  componentData.Item2.Ip, componentData.Item2.ServerId);
					continue;
				}
				ComponentManager.Default.AddRemoteComponent(componentData.Item2.ServerId,
				                                            ipAddress,
				                                            (ushort) componentData.Item2.Port,true);
			}
		}
	}
}