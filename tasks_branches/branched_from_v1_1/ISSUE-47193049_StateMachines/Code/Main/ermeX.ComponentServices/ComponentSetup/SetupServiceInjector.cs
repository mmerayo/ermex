using System;
using System.Diagnostics;
using Ninject;
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

		[Inject]
		private IStatusManager StatusManager { get; set; }


		public void InjectServices()
		{
			ConfigurationManager.SetSettingsSource(_settings);
			IoCManager.Kernel.Inject(this);
		}

		public void Reset()
		{
			//clean up the soa component and its reset to call the component manager
			if (IoCManager.Kernel == null) return;
			

			if (StatusManager.CurrentStatus != ComponentStatus.Running)
				return;
			try
			{
				StatusManager.CurrentStatus = ComponentStatus.Stopping;
			}
			catch (Exception)
			{
				IoCManager.Reset();
				throw;
			}
			IoCManager.Reset();
			SystemTaskQueue.Reset();
			StatusManager.CurrentStatus = ComponentStatus.Stopped;
		}
	}
}