using System;
using Ninject;
using Stateless;
using ermeX.ComponentServices.Interfaces.ComponentSetup;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Providers;
using ermeX.Versioning;

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

		[Inject]
		private IVersionUpgradeHelper VersionUpgradeHelper { get; set; }

		public void RunUpgrades()
		{
			IoCManager.Kernel.Inject(this);
			if (_settings.ConfigurationSourceType == DbEngineType.SqliteInMemory)
			{
				SessionProvider.SetInMemoryDb(_settings.ConfigurationConnectionString);
			}

			VersionUpgradeHelper.RunDataSchemaUpgrades(_settings.SchemasApplied,
													   _settings.ConfigurationConnectionString,
													   _settings.ConfigurationSourceType);
		}

	}
}