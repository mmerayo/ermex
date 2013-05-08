using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using ermeX.ComponentServices.ComponentSetup;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.ComponentServices
{
	internal sealed class ComponentManager
	{
		private readonly object _syncLock=new object();
		private SetupMachine _setupMachine;
		public static readonly ComponentManager Default=new ComponentManager();
		private ComponentManager()
		{}

		public void Setup(Configurer settings)
		{
			lock (_syncLock)
			{
				if (_setupMachine != null)
					throw new InvalidOperationException("The component can only be setup once per session");

				var componentSettings = settings.GetSettings<IComponentSettings>();
				ISetupServiceInjector serviceInjector=new SetupServiceInjector(componentSettings);
				ISetupVersionUpgradeRunner versionUpgrader=new SetupUpgradeRunner(settings.GetSettings<IDalSettings>());
				_setupMachine = new SetupMachine(serviceInjector,versionUpgrader);
			}
			_setupMachine.Setup();
		}
	}
}
