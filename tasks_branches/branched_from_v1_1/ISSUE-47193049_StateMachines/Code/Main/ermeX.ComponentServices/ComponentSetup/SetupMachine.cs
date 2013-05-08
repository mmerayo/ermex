﻿using System;
using Stateless;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupMachine
	{
		private readonly ISetupServiceInjector _serviceInjector;
		private readonly ISetupVersionUpgradeRunner _versionUpgrader;

		private enum SetupEvent
		{
			Inject,
			Injected,
			Upgrade,
			Upgraded,
			Error,
			Restart,
			Retry
		}

		private enum SetupProcessState
		{
			NotStarted = 0,
			InjectingServices,
			ServicesInjected,
			Upgrading,
			Ready,
			Error,
		}

		private readonly StateMachine<SetupProcessState, SetupEvent> _machine =
			new StateMachine<SetupProcessState, SetupEvent>(SetupProcessState.NotStarted);


		private StateMachine<SetupProcessState, SetupEvent>.TriggerWithParameters<Exception>
			_errorTrigger;

		public SetupMachine(ISetupServiceInjector serviceInjector,
		                    ISetupVersionUpgradeRunner versionUpgrader)
		{
			_serviceInjector = serviceInjector;
			_versionUpgrader = versionUpgrader;
			DefineStateMachineTransitions();

		}

		public void Setup()
		{
			try
			{
				if(IsReady()|| SetupFailed())
					Reset();
				else
					_machine.Fire(SetupEvent.Inject);
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
			        .Permit(SetupEvent.Inject, SetupProcessState.InjectingServices)
			        .Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.InjectingServices)
			        .OnEntry(OnInjectServices)
			        .Permit(SetupEvent.Injected, SetupProcessState.ServicesInjected)
			        .Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.ServicesInjected)
			        .OnEntry(OnServicesInjected)
			        .Permit(SetupEvent.Upgrade, SetupProcessState.Upgrading)
			        .Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.Upgrading)
			        .OnEntry(OnUpgrading)
			        .Permit(SetupEvent.Upgraded, SetupProcessState.Ready)
			        .Permit(SetupEvent.Error, SetupProcessState.Error);

			_machine.Configure(SetupProcessState.Ready)
				.OnEntry(OnReady)
				.Permit(SetupEvent.Restart, SetupProcessState.NotStarted); 

			_errorTrigger = _machine.SetTriggerParameters<Exception>(SetupEvent.Error);

			_machine.Configure(SetupProcessState.Error)
			        .OnEntryFrom(_errorTrigger, OnError)
			        .OnEntry(a => OnError(null))
					.Permit(SetupEvent.Retry, SetupProcessState.NotStarted);

		}

		private void OnReady(StateMachine<SetupProcessState, SetupEvent>.Transition obj)
		{
		}

		private void FireError(Exception ex)
		{
			if (SetupFailed())
				throw ex;
			_machine.Fire(_errorTrigger, ex);
		}

		private void TryFire(SetupEvent e)
		{
			if (_machine == null)
				throw new InvalidOperationException("Invoke SetMachine first");
			if (!_machine.CanFire(e))
				throw new InvalidOperationException(string.Format("Cannot transit from:{0} with trigger:{1}", _machine.State,e));
			if (e == SetupEvent.Error)
				throw new InvalidOperationException("Use FireError");
			_machine.Fire(e);
		}

		private void OnNotStarted(
			StateMachine<SetupProcessState, SetupEvent>.Transition transtitionData)
		{
			try
			{
				_serviceInjector.Reset();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(SetupEvent.Inject);

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
			TryFire(SetupEvent.Upgrade);
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


		public void Reset()
		{
			if (SetupFailed())
				TryFire(SetupEvent.Retry);
			else
				TryFire(SetupEvent.Restart);
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

		public bool IsNotStarted()
		{
			return _machine.State == SetupProcessState.NotStarted;
		}
	}
}