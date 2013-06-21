using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Subscriptions;

namespace ermeX.ComponentServices.RemoteComponent.Commands
{
	class OnRequestingSubscriptionsStepExecutor : IOnRequestingSubscriptionsStepExecutor
	{
		private readonly IMessagePublisher _publisher;
		private readonly IComponentSettings _settings;
		private readonly ICanUpdateOutgoingMessagesSubscriptions _outgoingMessagesSubscriptionsWritter;
		private static readonly ILog Logger = LogManager.GetLogger<OnRequestingSubscriptionsStepExecutor>();

		[Inject]
		public OnRequestingSubscriptionsStepExecutor(IMessagePublisher publisher,
		                                        IComponentSettings settings,
			ICanUpdateOutgoingMessagesSubscriptions outgoingMessagesSubscriptionsWritter )
		{
			_publisher = publisher;
			_settings = settings;
			_outgoingMessagesSubscriptionsWritter = outgoingMessagesSubscriptionsWritter;
		}

		public void Request(IRemoteComponentStateMachineContext context)
		{
			Logger.DebugFormat("Request- Component:", context.RemoteComponentId);

			//TODO: MOVE PAYLOAD

			var proxy = _publisher.GetServiceProxy<IMessageSuscriptionsService>(context.RemoteComponentId);

			var request = new MessageSuscriptionsRequestMessage(context.RemoteComponentId);
			var remoteSuscriptions = proxy.RequestSuscriptions(request);

			//remote incomming is local outgoing
			if (remoteSuscriptions.MyIncomingSuscriptions != null)
				_outgoingMessagesSubscriptionsWritter.ImportFromOtherComponent(remoteSuscriptions.MyIncomingSuscriptions);

			//remote outgoing is local outgoing but local subscriptions
			if (remoteSuscriptions.MyOutgoingSuscriptions != null)
			{
				var outgoingMessageSuscriptions =
					remoteSuscriptions.MyOutgoingSuscriptions.Where(x => x.Component != context.RemoteComponentId);
				_outgoingMessagesSubscriptionsWritter.ImportFromOtherComponent(outgoingMessageSuscriptions);
			}
		}
	}
}