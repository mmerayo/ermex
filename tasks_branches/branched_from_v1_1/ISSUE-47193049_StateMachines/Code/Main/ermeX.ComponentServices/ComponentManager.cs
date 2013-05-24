using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Stateless;
using ermeX.ComponentServices.ComponentSetup;
using ermeX.ComponentServices.LocalComponent;
using ermeX.ComponentServices.RemoteComponent;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;

namespace ermeX.ComponentServices
{
	internal sealed class ComponentManager
	{
		private readonly object _syncLock=new object();
		private SetupMachine _setupMachine;
		public static readonly ComponentManager Default=new ComponentManager();
		private volatile ILocalComponent _localComponent;
		private volatile IRemoteComponent _friendComponent;
		private Configurer _settings;
		private ComponentManager()
		{}
		
		public void Setup(Configurer settings)
		{
			lock (_syncLock)
			{
				_settings = settings;
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
				IoCManager.InjectObject(this);
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

		public IRemoteComponent FriendComponent
		{
			get
			{
				if(!IsRunning()) throw new InvalidOperationException();

				if(_friendComponent==null)
					lock(_syncLock)
						if (_friendComponent == null)
						{
							//TODO: components are created prelive and then invoke join from stop in a different thread
							//TODO: when any component is errored establish a policy to try to restart it
							//throw new NotImplementedException("The following lines to be implemented by the state machine");
							var busSettings = _settings.GetSettings<IBusSettings>();
							if (busSettings.FriendComponent != null)
							{
								_friendComponent = (IRemoteComponent) IoCManager.Kernel.GetService(typeof (IRemoteComponent));
								_friendComponent.Create(busSettings.FriendComponent.ComponentId,
								                        busSettings.FriendComponent.Endpoint.Address,
								                        busSettings.FriendComponent.Endpoint.Port);
							}
								
						}

				throw new NotImplementedException("return value");
				return null;
			}
		}
	}
}
