using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Ninject;
using Stateless;
using ermeX.ComponentServices.RemoteComponent.Commands;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal sealed class RemoteComponentStateMachine:IRemoteComponentStateMachine
	{
		private readonly IOnPreliveExecutor _preliveExecutor;
		private readonly IOnCreatingStepExecutor _creatingExecutor;
		private readonly IOnStoppedStepExecutor _stoppedStepExecutor;
		private readonly IOnJoiningStepExecutor _joiningStepExecutor;
		private readonly IOnRunningStepExecutor _runningStepExecutor;
		private readonly IOnErrorStepExecutor _errorStepExecutor;

		private enum RemoteComponentEvent
		{
			Create=0,
			Ready,
			Join,
			Joined,
			ToError,
			Stop
		}
		private enum RemoteComponentState
		{
			Prelive=0,
			Creating,
			Stopped,
			Joining,
			Running,
			Errored
		}
		private static readonly ILog Logger = LogManager.GetLogger<RemoteComponentStateMachine>();

		private readonly StateMachine<RemoteComponentState, RemoteComponentEvent> _machine =
			new StateMachine<RemoteComponentState, RemoteComponentEvent>(RemoteComponentState.Prelive);

		private StateMachine<RemoteComponentState, RemoteComponentEvent>.TriggerWithParameters<Exception> _errorTrigger;

		[Inject]
		public RemoteComponentStateMachine(IOnPreliveExecutor preliveExecutor,
			IOnCreatingStepExecutor creatingExecutor,
			IOnStoppedStepExecutor stoppedStepExecutor,
			IOnJoiningStepExecutor joiningStepExecutor,
			IOnRunningStepExecutor runningStepExecutor,
			IOnErrorStepExecutor errorStepExecutor)
		{
			_preliveExecutor = preliveExecutor;
			_creatingExecutor = creatingExecutor;
			_stoppedStepExecutor = stoppedStepExecutor;
			_joiningStepExecutor = joiningStepExecutor;
			_runningStepExecutor = runningStepExecutor;
			_errorStepExecutor = errorStepExecutor;
			DefineStateMachineTransitions();
		}

		private void DefineStateMachineTransitions()
		{
			Logger.Debug("DefineStateMachineTransitions");
			_machine.Configure(RemoteComponentState.Prelive)
					.OnEntry(OnPrelive)
					.Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
					.Permit(RemoteComponentEvent.Create, RemoteComponentState.Creating);

			_machine.Configure(RemoteComponentState.Creating)
					.OnEntry(OnCreating)
					.Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
					.Permit(RemoteComponentEvent.Ready, RemoteComponentState.Stopped);

			_machine.Configure(RemoteComponentState.Stopped)
			        .OnEntry(OnStopped)
					.Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
			        .Permit(RemoteComponentEvent.Join, RemoteComponentState.Joining);

			_machine.Configure(RemoteComponentState.Joining)
			        .OnEntry(OnJoining)
			        .Permit(RemoteComponentEvent.Joined, RemoteComponentState.Running)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored);

			_machine.Configure(RemoteComponentState.Running)
			        .OnEntry(OnRunning)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
			        .Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped);
			
			_errorTrigger = _machine.SetTriggerParameters<Exception>(RemoteComponentEvent.ToError);

			_machine.Configure(RemoteComponentState.Errored)
			        .OnEntryFrom(_errorTrigger, OnError)
			        .OnEntry(a => OnError(null));
		}

		private void FireError(Exception ex)
		{
			Logger.WarnFormat("FireError-{0}", ex.ToString());
			if (IsErrored())
				throw ex;
			_machine.Fire(_errorTrigger, ex);
		}


		private void TryFire(RemoteComponentEvent e)
		{
			Logger.DebugFormat("TryFire - {0}", e);
			if (_machine == null)
				throw new ApplicationException("FATAL");
			if (!_machine.CanFire(e))
				throw new InvalidOperationException(string.Format("Cannot transit from:{0} with trigger:{1}", _machine.State, e));
			if (e == RemoteComponentEvent.ToError)
				throw new InvalidOperationException("Use FireError");
			_machine.Fire(e);
		}

		private void OnError(Exception ex)
		{
			if (ex == null) throw new ArgumentNullException("ex");
			Logger.DebugFormat("OnError - {0}", ex.ToString());
			_errorStepExecutor.OnError(); //TODO: MOVE NEXT LINE TO THE EXECUTOR
			throw new ermeXRemoteComponentException(ex);
		}


		private void OnRunning(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnJoined-{0}", obj.Trigger);
			try
			{
				_runningStepExecutor.OnRunning();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnJoining(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnJoining-{0}", obj.Trigger);
			try
			{
				_joiningStepExecutor.Join();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnStopped(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnStopped-{0}", obj.Trigger);
			try
			{
				_stoppedStepExecutor.Stop();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnCreating(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnCreating-{0}", obj.Trigger);
			try
			{
				_creatingExecutor.Create();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(RemoteComponentEvent.Ready);
		}

		private void OnPrelive(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnPrelive-{0}", obj.Trigger);
			try
			{
				_preliveExecutor.OnPrelive();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		public void Create()
		{
			TryFire(RemoteComponentEvent.Create);
		}

		public void Join()
		{
			TryFire(RemoteComponentEvent.Join);
		}

		public void Joined()
		{
			TryFire(RemoteComponentEvent.Joined);
		}

		public void Stop()
		{
			TryFire(RemoteComponentEvent.Stop);
		}

		public bool IsErrored()
		{
			return _machine.State == RemoteComponentState.Errored;
		}

		public bool IsStopped()
		{
			return _machine.State == RemoteComponentState.Stopped;
		}

		public bool IsRunning()
		{
			return _machine.State == RemoteComponentState.Running;
		}
	}
}
