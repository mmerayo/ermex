using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using ermeX.ComponentServices.ComponentSetup;
using ermeX.ComponentServices.LocalComponent;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.ComponentServices
{
	internal sealed class ComponentManager
	{
		private readonly object _syncLock=new object();
		private SetupMachine _setupMachine;
		public static readonly ComponentManager Default=new ComponentManager();
		private volatile ILocalComponent _localComponent;

		private ComponentManager()
		{}

		public void Setup(Configurer settings)
		{
			lock (_syncLock)
			{
				if (_setupMachine != null)
					Reset();
				else
				{
					var componentSettings = settings.GetSettings<IComponentSettings>();
					ISetupServiceInjector serviceInjector = new SetupServiceInjector(componentSettings);
					ISetupVersionUpgradeRunner versionUpgrader = new SetupUpgradeRunner(settings.GetSettings<IDalSettings>());
					_setupMachine = new SetupMachine(serviceInjector, versionUpgrader);
				}
				_setupMachine.Setup();
			}
		}

		public void Reset()
		{
			lock (_syncLock)
			{
				if (_setupMachine != null)
					_setupMachine.Reset();

				//TODO: DISPOSE THE COMPONENTS
			}
		}

		public bool IsRunning()
		{
			return _setupMachine != null && _setupMachine.IsReady();
		}

		public ILocalComponent LocalComponent
		{
			get
			{
				if(!IsRunning()) throw new InvalidOperationException();
				
				if (_localComponent == null)
					lock (_syncLock)
						if (_localComponent == null)
							_localComponent = (ILocalComponent)IoCManager.Kernel.GetService(typeof(ILocalComponent));
				return _localComponent;
			}
		} 
	}
}
