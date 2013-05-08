using System;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings;

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
	}
}