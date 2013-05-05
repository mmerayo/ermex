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

		private enum SetupProcessState
		{
			NotStarted = 0,
			InjectingServices,
			ServicesInjected,
			Upgrading,
			Ready,
			Error
		}

		private enum SetupEvent
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
		}

		public void Setup()
		{
			DefineStateMachineTransitions();

			_machine.Fire(SetupEvent.Inject);
		}

		private void DefineStateMachineTransitions()
		{
			_machine.Configure(SetupProcessState.NotStarted)
				.Permit(SetupEvent.Inject, SetupProcessState.InjectingServices);

			_machine.Configure(SetupProcessState.InjectingServices)
				.OnEntry(_payloader.InjectServices)
				.Permit(SetupEvent.Injected, SetupProcessState.ServicesInjected)
				.Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.ServicesInjected)
				.Permit(SetupEvent.Upgrade, SetupProcessState.Upgrading);

			_machine.Configure(SetupProcessState.Upgrading)
				.OnEntry(_payloader.RunUpgrades)
				.Permit(SetupEvent.Upgraded, SetupProcessState.Ready)
				.Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.Error)
				.OnEntry(_payloader.HandleError);
		}
	}
}
