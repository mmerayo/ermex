using System;
using System.Net;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.Common;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Logging;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnJoiningStepExecutor : IOnJoiningStepExecutor
	{
		private readonly ILogger _logger ;

		private readonly IMessagePublisher _publisher;
		private readonly IComponentSettings _settings;
		private readonly IBizSettings _bizSettings;

		[Inject]
		public OnJoiningStepExecutor(
			IMessagePublisher publisher,
			IComponentSettings settings,
			IBizSettings bizSettings)
		{
			_logger = LogManager.GetLogger<OnJoiningStepExecutor>(settings.ComponentId,LogComponent.Handshake);
			_publisher = publisher;
			_settings = settings;
			_bizSettings = bizSettings;
		}

		public void Join(IRemoteComponentStateMachineContext context)
		{
			_logger.DebugFormat("Join- Component:", context.RemoteComponentId);

			var handshakeService = _publisher.GetServiceProxy<IHandshakeService>(context.RemoteComponentId);
			var message = new JoinRequestMessage(_settings.ComponentId,
			                                     Networking.GetLocalhostIp(),//TODO: CHECK THE CORRECTNESS OF THIS WHEN WORKING BEHIND A ROUTER
			                                     _bizSettings.TcpPort);
			MyComponentsResponseMessage response = null;
			try
			{
				response = handshakeService.RequestJoinNetwork(message);
			}
			catch (Exception ex)
			{
				_logger.Warn(x => x("Join - Could not join the component {0}. Reason {1}", context.RemoteComponentId, ex));
				throw;
			}
			AddComponentsFromResponse(response);
		}

		private void AddComponentsFromResponse(MyComponentsResponseMessage response)
		{
			foreach (var componentData in response.Components)
			{
				IPAddress ipAddress;
				if (!IPAddress.TryParse(componentData.Item2.Ip, out ipAddress))
				{
					_logger.WarnFormat("Join - Component not recognized. Could not parse the IP: {0} of the Component: {1}",
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