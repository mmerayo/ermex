using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Configuration;

namespace ermeX.ComponentServices.LocalComponent.Commands
{
	class OnPublishServicesStepExecutor : IOnPublishServicesStepExecutor
	{
		private readonly Configurer _settings;
		private readonly IServicePublishingManager _servicePublishingManager;

		[Inject]
		public OnPublishServicesStepExecutor(Configurer settings,
			IServicePublishingManager servicePublishingManager)
		{
			_settings = settings;
			_servicePublishingManager = servicePublishingManager;
		}

		public void Publish()
		{
			var discoveredServices = _settings.GetDiscoveredServices();
			foreach (var svc in discoveredServices)
			{
				_servicePublishingManager.PublishService(svc.InterfaceType, svc.ImplementationType);
			}
		}
	}
}