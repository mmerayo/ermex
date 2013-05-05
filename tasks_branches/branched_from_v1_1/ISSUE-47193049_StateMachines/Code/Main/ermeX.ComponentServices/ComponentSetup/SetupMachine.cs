using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupMachine
	{
		private readonly ISetupPayloader _payloader;

		public enum SetupProcessState
		{
			NotStarted = 0,
			InjectingServices,
			ServicesInjected,
			Upgrading,
			Ready,
			Error
		}

		public enum SetupEvent
		{
			Inject,
			Injected,
			Upgrade,
			Upgraded,
			Error
		}

		private readonly StateMachine<SetupProcessState, SetupEvent> _machine= new StateMachine<SetupProcessState, SetupEvent>(SetupProcessState.NotStarted);

		public SetupMachine(ISetupPayloader payloader)
		{
			if (payloader == null) throw new ArgumentNullException("payloader");
			_payloader = payloader;
			_payloader.SetMachine(_machine);
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
				.OnEntry((e)=>_payloader.TryFire(SetupEvent.Inject))
				.Permit(SetupEvent.Inject, SetupProcessState.InjectingServices);

			_machine.Configure(SetupProcessState.InjectingServices)
				.OnEntry(_payloader.InjectServices)
				.Permit(SetupEvent.Injected, SetupProcessState.ServicesInjected)
				.Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.ServicesInjected)
				.OnEntry((a)=> _payloader.TryFire(SetupEvent.Upgrade))
				.Permit(SetupEvent.Upgrade, SetupProcessState.Upgrading);

			_machine.Configure(SetupProcessState.Upgrading)
				.OnEntry(_payloader.RunUpgrades)
				.Permit(SetupEvent.Upgraded, SetupProcessState.Ready)
				.Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.Error)
				.OnEntryFrom(_errorTrigger, ex => _payloader.HandleError(ex))
				.OnEntry(a=>_payloader.HandleError(null));

			_errorTrigger = _machine.SetTriggerParameters<Exception>(SetupEvent.Error);
		}

		private StateMachine<SetupMachine.SetupProcessState, SetupMachine.SetupEvent>.TriggerWithParameters<Exception> _errorTrigger;

		public void FireError(Exception ex) // TODO: REFACTOR THIS
		{
			_machine.Fire(_errorTrigger, ex);
		}
	}
}
