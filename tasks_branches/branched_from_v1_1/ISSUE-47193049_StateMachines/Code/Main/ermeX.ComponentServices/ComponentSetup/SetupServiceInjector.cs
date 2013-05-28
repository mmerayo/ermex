using System;
using System.Diagnostics;
using Ninject;
using ermeX.ComponentServices.Interfaces;
using ermeX.ComponentServices.Interfaces.ComponentSetup;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;

using ermeX.Parallel.Queues;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupServiceInjector : ISetupServiceInjector
	{
		private readonly IComponentSettings _settings;

		public SetupServiceInjector(IComponentSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			_settings = settings;
		}

		public void InjectServices()
		{
			ConfigurationManager.SetSettingsSource(_settings);
		}

		public void Reset()
		{
			//clean up the soa component and its reset to call the component manager
			if (IoCManager.Kernel == null) return;
			
			IoCManager.Reset();
			IoCManager.Kernel.Bind<IComponentManager>().ToConstant(ComponentManager.Default);//TODO: INJECT THIS PREVIOUSLY AND REINJECT AFTER EVERY RESET
			SystemTaskQueue.Reset();
		}
	}
}