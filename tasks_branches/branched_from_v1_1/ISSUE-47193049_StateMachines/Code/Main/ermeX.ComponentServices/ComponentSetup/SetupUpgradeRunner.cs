using System;
using Stateless;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupUpgradeRunner:ISetupVersionUpgradeRunner
	{
		private readonly IDalSettings _settings;

		public SetupUpgradeRunner(IDalSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			_settings = settings;
		}

		public void RunUpgrades()
		{
			throw new System.NotImplementedException();
		}

	}
}