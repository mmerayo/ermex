using System;
using System.Diagnostics;
using Common.Logging;
using Ninject;
using ermeX.ComponentServices.Interfaces;
using ermeX.ComponentServices.Interfaces.ComponentSetup;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;

using ermeX.Parallel.Queues;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupServiceInjector : ISetupServiceInjector
	{
		private static readonly ILog Logger = LogManager.GetLogger<SetupServiceInjector>();
		private readonly Configurer _settings;

		public SetupServiceInjector(Configurer settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;
		}

		public void InjectServices()
		{
			var componentSettings = _settings.GetSettings<IComponentSettings>();
			Logger.DebugFormat("Component:{0} - InjectServices",componentSettings.ComponentId);
			ConfigurationManager.SetSettingsSource(componentSettings);
			InjectConstantSettings();
		}

		public void Reset()
		{
			Logger.Debug("Reset");

			//clean up the soa component and its reset to call the component manager
			if (IoCManager.Kernel == null) return;
			
			IoCManager.Reset();

			////TODO: INJECT THIS PREVIOUSLY AND REINJECT AFTER EVERY RESET
			//InjectConstantSettings();

			SystemTaskQueue.Reset();
		}

		private void InjectConstantSettings()
		{
			IoCManager.Kernel.Bind<Configurer>().ToConstant(_settings);
			IoCManager.Kernel.Bind<IComponentManager>().ToConstant(ComponentManager.Default);
		}
	}
}