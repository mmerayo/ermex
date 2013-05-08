using System;
using Stateless;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupMachine
	{
		private readonly ISetupServiceInjector _serviceInjector;
		private readonly ISetupVersionUpgradeRunner _versionUpgrader;
		private readonly ISetupConfigureComponent _configurator;

		private enum SetupEvent
		{
			Inject,
			Injected,
			Upgrade,
			Upgraded,
			Error,
			Configure
		}

		private enum SetupProcessState
		{
			NotStarted = 0,
			InjectingServices,
			ServicesInjected,
			Upgrading,
			Ready,
			Error,
			Configuring
		}

		private readonly StateMachine<SetupProcessState, SetupEvent> _machine =
			new StateMachine<SetupProcessState, SetupEvent>(SetupProcessState.NotStarted);


		private StateMachine<SetupProcessState, SetupEvent>.TriggerWithParameters<Exception>
			_errorTrigger;

		public SetupMachine(ISetupServiceInjector serviceInjector, 
			ISetupVersionUpgradeRunner versionUpgrader,
			ISetupConfigureComponent configurator)
		{
			_serviceInjector = serviceInjector;
			_versionUpgrader = versionUpgrader;
			_configurator = configurator;
		}

		public void Setup()
		{
			DefineStateMachineTransitions();
			try{
			_machine.Fire(SetupEvent.Configure);
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void DefineStateMachineTransitions()
		{
			_machine.Configure(SetupProcessState.NotStarted)
			        .OnEntry(OnNotStarted)
			        .Permit(SetupEvent.Configure, SetupProcessState.Configuring);
			_machine.Configure(SetupProcessState.Configuring)
			        .OnEntry(OnConfiguring)
			        .Permit(SetupEvent.Inject, SetupProcessState.InjectingServices);

			_machine.Configure(SetupProcessState.InjectingServices)
			        .OnEntry(OnInjectServices)
			        .Permit(SetupEvent.Injected, SetupProcessState.ServicesInjected)
			        .Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.ServicesInjected)
			        .OnEntry(OnServicesInjected)
			        .Permit(SetupEvent.Upgrade, SetupProcessState.Upgrading);

			_machine.Configure(SetupProcessState.Upgrading)
			        .OnEntry(OnUpgrading)
			        .Permit(SetupEvent.Upgraded, SetupProcessState.Ready)
			        .Permit(SetupEvent.Error, SetupProcessState.Error);

			_errorTrigger = _machine.SetTriggerParameters<Exception>(SetupEvent.Error);

			_machine.Configure(SetupProcessState.Error)
			        .OnEntryFrom(_errorTrigger, OnError)
			        .OnEntry(a => OnError(null));

		}
		private void FireError(Exception ex)
		{
			_machine.Fire(_errorTrigger, ex);
		}

		private void TryFire(SetupEvent e)
		{
			if (_machine == null)
				throw new InvalidOperationException("Invoke SetMachine first");
			if (!_machine.CanFire(e))
				throw new InvalidOperationException(string.Format("Cannot transit from {0}", _machine.State));
			if (e == SetupEvent.Error)
				throw new InvalidOperationException("Use FireError");
			_machine.Fire(e);
		}

		private void OnNotStarted(
			StateMachine<SetupProcessState, SetupEvent>.Transition transtitionData)
		{
			try
			{
				TryFire(SetupEvent.Configure);
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnConfiguring(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			try
			{
				_configurator.Configure();
				TryFire(SetupEvent.Inject);
			}
			catch (Exception ex)
			{
				FireError(ex);
			}

		}

		private void OnInjectServices(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			try
			{
				_serviceInjector.InjectServices();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(SetupEvent.Injected);
		}

		private void OnServicesInjected(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			try
			{
				TryFire(SetupEvent.Upgrade);
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnUpgrading(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			try
			{
				_versionUpgrader.RunUpgrades();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(SetupEvent.Upgraded);
		}

		public void OnError(Exception ex)
		{
			if (ex == null) throw new ArgumentNullException("ex");

			//TODO: LOG??
			throw new ermeXSetupException(ex);
		}

		public bool IsReady()
		{
			return _machine.State == SetupProcessState.Ready;
		}

		public bool SetupFailed()
		{
			return _machine.State == SetupProcessState.Error;
		}
	}
}