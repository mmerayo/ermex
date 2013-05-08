using System;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal class SetupComponentConfigurator : ISetupConfigureComponent
	{
		private IComponentSettings _settings;

		public SetupComponentConfigurator(IComponentSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			_settings = settings;
		}
		public void Configure()
		{
			throw new NotImplementedException();
		}
	}
}