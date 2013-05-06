using System;
using Stateless;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupMachine
	{
		public enum SetupEvent
		{
			Inject,
			Injected,
			Upgrade,
			Upgraded,
			Error
		}

		public enum SetupProcessState
		{
			NotStarted = 0,
			InjectingServices,
			ServicesInjected,
			Upgrading,
			Ready,
			Error
		}

		private readonly StateMachine<SetupProcessState, SetupEvent> _machine =
			new StateMachine<SetupProcessState, SetupEvent>(SetupProcessState.NotStarted);

		private readonly ISetupPayloader _payloader;

		private StateMachine<SetupProcessState, SetupEvent>.TriggerWithParameters<Exception>
			_errorTrigger;

		public SetupMachine(ISetupPayloader payloader)
		{
			if (payloader == null) throw new ArgumentNullException("payloader");
			_payloader = payloader;
		}

		public SetupProcessState State
		{
			get { return _machine.State; }
		}

		public void Setup()
		{
			DefineStateMachineTransitions();

			_machine.Fire(SetupEvent.Inject);
		}

		private void DefineStateMachineTransitions()
		{
			_machine.Configure(SetupProcessState.NotStarted)
				.OnEntry(OnNotStarted)
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
			TryFire(SetupEvent.Inject);
		}

		private void OnInjectServices(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			try
			{
				_payloader.InjectServices();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(SetupEvent.Injected);
		}

		private void OnServicesInjected(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			TryFire(SetupEvent.Upgrade);
		}

		private void OnUpgrading(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			try
			{
				_payloader.RunUpgrades();
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
	}
}