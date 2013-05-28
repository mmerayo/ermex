using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ninject;
using Stateless;
using ermeX.ComponentServices.ComponentSetup;
using ermeX.ComponentServices.Interfaces;
using ermeX.ComponentServices.Interfaces.ComponentSetup;
using ermeX.ComponentServices.Interfaces.LocalComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.LocalComponent;
using ermeX.ComponentServices.RemoteComponent;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;
using ermeX.Exceptions;

namespace ermeX.ComponentServices
{
	internal sealed class ComponentManager : IComponentManager
	{
		private readonly object _syncLock=new object();
		private SetupMachine _setupMachine;
		public static readonly IComponentManager Default=new ComponentManager();//TODO: INJECT THIS PREVIOUSLY TO THE REST OF THE INJECTIONS
		private volatile ILocalComponent _localComponent;
		private volatile IRemoteComponent _friendComponent;
		private readonly IDictionary<Guid,IRemoteComponent> _remoteComponents = new Dictionary<Guid, IRemoteComponent>();
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
				if (_localComponent != null && !_localComponent.IsRunning())
					throw new InvalidOperationException("The has not been started");

				if (_setupMachine != null)
					_setupMachine.Reset();

				if (_localComponent != null)
						_localComponent.Stop();
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
								AddRemoteComponent(busSettings.FriendComponent.ComponentId,
			                        busSettings.FriendComponent.Endpoint.Address,
			                        (ushort) busSettings.FriendComponent.Endpoint.Port);
								_friendComponent = GetRemoteComponent(busSettings.FriendComponent.ComponentId);
							}
						}

				return _friendComponent;
			}
		}

		public IRemoteComponent GetRemoteComponent(Guid componentId)
		{
			IRemoteComponent result;
			_remoteComponents.TryGetValue(componentId, out result);

			return result;
		}

		public bool AddRemoteComponent(Guid componentId, IPAddress address, ushort port,bool joinIfCreated=false)
		{

			if(!_remoteComponents.ContainsKey(componentId))
				lock(_syncLock)
					if (!_remoteComponents.ContainsKey(componentId))
					{
						var result = (IRemoteComponent)IoCManager.Kernel.GetService(typeof(IRemoteComponent));
						_remoteComponents.Add(componentId,result);
						result.Create(componentId, address, port);
						if(joinIfCreated)
							result.Join();
						return true;
					}

			return false;
		}
	}
}
