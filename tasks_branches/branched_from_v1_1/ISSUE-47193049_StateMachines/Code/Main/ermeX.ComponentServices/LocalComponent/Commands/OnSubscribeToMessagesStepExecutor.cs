using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Configuration;

namespace ermeX.ComponentServices.LocalComponent.Commands
{
	class OnSubscribeToMessagesStepExecutor : IOnSubscribeToMessagesStepExecutor
	{
		private readonly Configurer _settings;
		private readonly ISubscriptionsManager _subscriptionsManager;

		[Inject]
		public OnSubscribeToMessagesStepExecutor(Configurer settings,
			ISubscriptionsManager subscriptionsManager)
		{
			_settings = settings;
			_subscriptionsManager = subscriptionsManager;
		}

		public void Subscribe()
		{ 
			var discoveredSubscriptions =_settings.GetDiscoveredSubscriptions();
			foreach (var discoveredSubscription in discoveredSubscriptions)
			{
				_subscriptionsManager.Subscribe(discoveredSubscription.HandlerType,
														discoveredSubscription.MessageType);
			}
		}
	}
}