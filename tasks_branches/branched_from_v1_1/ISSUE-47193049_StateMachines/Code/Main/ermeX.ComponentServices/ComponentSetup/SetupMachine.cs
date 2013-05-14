using System;
using System.Diagnostics;
using Common.Logging;
using Stateless;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.ComponentSetup
{
	internal sealed class SetupMachine
	{
		private static readonly ILog Logger = LogManager.GetLogger<SetupMachine>();

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

		private readonly ISetupServiceInjector _serviceInjector;
		private readonly ISetupVersionUpgradeRunner _versionUpgrader;
		public SetupMachine(ISetupServiceInjector serviceInjector,
		                    ISetupVersionUpgradeRunner versionUpgrader)
		{
			Logger.Debug("cctor");
			_serviceInjector = serviceInjector;
			_versionUpgrader = versionUpgrader;
			DefineStateMachineTransitions();

		}

		public void Setup()
		{
			try
			{
				Logger.Debug("Setup");
				if(IsReady()|| SetupFailed())
					Reset();
				Debug.Assert(IsNotStarted());
				_machine.Fire(SetupEvent.Inject);
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void DefineStateMachineTransitions()
		{
			Logger.Debug("DefineStateMachineTransitions");
			_machine.Configure(SetupProcessState.NotStarted)
			        .OnExit(OnNotStartedExit)
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
			Logger.DebugFormat("OnReady-{0}",obj);
		}

		private void FireError(Exception ex)
		{
			Logger.WarnFormat("FireError-{0}", ex.ToString());
			if (SetupFailed())
				throw ex;
			_machine.Fire(_errorTrigger, ex);
		}

		private void TryFire(SetupEvent e)
		{
			Logger.DebugFormat("TryFire - {0}", e);
			if (_machine == null)
				throw new InvalidOperationException("Invoke SetMachine first");
			if (!_machine.CanFire(e))
				throw new InvalidOperationException(string.Format("Cannot transit from:{0} with trigger:{1}", _machine.State,e));
			if (e == SetupEvent.Error)
				throw new InvalidOperationException("Use FireError");
			_machine.Fire(e);
		}

		private void OnNotStartedExit(
			StateMachine<SetupProcessState, SetupEvent>.Transition transtitionData)
		{
			Logger.DebugFormat("OnNotStartedExit-{0}", transtitionData.Trigger);
			try
			{
				_serviceInjector.Reset();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnInjectServices(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			Logger.DebugFormat("OnInjectServices-{0}", transition.Trigger);
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
			Logger.DebugFormat("OnServicesInjected-{0}" , transition.Trigger);
			TryFire(SetupEvent.Upgrade);
		}

		private void OnUpgrading(StateMachine<SetupProcessState, SetupEvent>.Transition transition)
		{
			Logger.DebugFormat("OnUpgrading-{0}", transition.Trigger);
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
			Logger.Debug("Reset");
			
			if (IsNotStarted())
				return;

			if (SetupFailed())
				TryFire(SetupEvent.Retry);
			else
				TryFire(SetupEvent.Restart);
		}


		public void OnError(Exception ex)
		{
			if (ex == null) throw new ArgumentNullException("ex");
			Logger.DebugFormat("OnError - {0}", ex.ToString());
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