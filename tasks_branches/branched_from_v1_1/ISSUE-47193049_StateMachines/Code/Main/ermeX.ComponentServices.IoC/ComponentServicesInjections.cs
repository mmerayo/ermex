using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using ermeX.ComponentServices.Interfaces.LocalComponent;
using ermeX.ComponentServices.Interfaces.LocalComponent.Commands;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ComponentServices.LocalComponent;
using ermeX.ComponentServices.LocalComponent.Commands;
using ermeX.ComponentServices.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.Settings;
using IOnErrorStepExecutor = ermeX.ComponentServices.Interfaces.RemoteComponent.Commands.IOnErrorStepExecutor;
using OnErrorStepExecutor = ermeX.ComponentServices.RemoteComponent.Commands.OnErrorStepExecutor;

namespace ermeX.ComponentServices.IoC
{
	internal class ComponentServicesInjections : NinjectModule
	{
		private readonly IComponentSettings _settings;

		public ComponentServicesInjections(IComponentSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			_settings = settings;
		}

		public override void Load()
		{
			BindLocalComponentServices();
			BindRemoteComponentServices();
		}

		private void BindLocalComponentServices()
		{
			Bind<ILocalComponent>().To<LocalComponent.LocalComponent>();
			Bind<ILocalComponentStateMachine>().To<LocalComponentStateMachine>();

			Bind<ermeX.ComponentServices.Interfaces.LocalComponent.Commands.IOnErrorStepExecutor>().To<ermeX.ComponentServices.LocalComponent.Commands.OnErrorStepExecutor>();
			Bind<IOnPublishServicesStepExecutor>().To<OnPublishServicesStepExecutor>();
			Bind<IOnResetStepExecutor>().To<OnResetStepExecutor>();
			Bind<IOnRunStepExecutor>().To<OnRunStepExecutor>();
			Bind<IOnStartStepExecutor>().To<OnStartStepExecutor>();
			Bind<IOnStopStepExecutor>().To<OnStopStepExecutor>();
			Bind<IOnSubscribeToMessagesStepExecutor>().To<OnSubscribeToMessagesStepExecutor>();

		}

		private void BindRemoteComponentServices()
		{
			Bind<IRemoteComponent>().To<RemoteComponent.RemoteComponent>();
			Bind<IRemoteComponentStateMachine>().To<RemoteComponent.RemoteComponentStateMachine>();
			Bind<IRemoteComponentStateMachineContext>().To<RemoteComponent.RemoteComponentStateMachineContext>();

			Bind<IOnCreatingStepExecutor>().To<OnCreatingStepExecutor>();
			Bind<IOnErrorStepExecutor>().To<OnErrorStepExecutor>();
			Bind<IOnJoiningStepExecutor>().To<OnJoiningStepExecutor>();
			Bind<IOnPreliveExecutor>().To<OnPreliveExecutor>();

			Bind<IOnRequestingServicesStepExecutor>().To<OnRequestingServicesStepExecutor>();
			Bind<IOnRequestingSubscriptionsStepExecutor>().To<OnRequestingSubscriptionsStepExecutor>();

			Bind<IOnRunningStepExecutor>().To<OnRunningStepExecutor>();
			Bind<IOnStoppedStepExecutor>().To<OnStoppedStepExecutor>();

			Bind<IOnServicesReceivedStepExecutor>().To<OnServicesReceivedStepExecutor>();
			Bind<IOnSubscriptionsReceivedStepExecutor>().To<OnSubscriptionsReceivedStepExecutor>();
			
		}
	}
}
