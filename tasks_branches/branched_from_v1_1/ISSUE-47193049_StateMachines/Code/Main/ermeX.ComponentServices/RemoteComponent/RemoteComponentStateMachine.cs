using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Ninject;
using Stateless;
using ermeX.ComponentServices.LocalComponent;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal sealed class RemoteComponentStateMachine:IRemoteComponentStateMachine
	{
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
			Created,
			Stopped,
			Joining,
			Joined,
			Errored
		}
		private static readonly ILog Logger = LogManager.GetLogger<RemoteComponentStateMachine>();

		private readonly StateMachine<RemoteComponentState, RemoteComponentEvent> _machine =
			new StateMachine<RemoteComponentState, RemoteComponentEvent>(RemoteComponentState.Prelive);

		private StateMachine<RemoteComponentState, RemoteComponentEvent>.TriggerWithParameters<Exception> _errorTrigger;

		[Inject]
		public RemoteComponentStateMachine()
		{
			DefineStateMachineTransitions();
		}

		private void DefineStateMachineTransitions()
		{
			Logger.Debug("DefineStateMachineTransitions");
			_machine.Configure(RemoteComponentState.Prelive)
					.OnEntry(OnPrelive)
					.Permit(RemoteComponentEvent.Create, RemoteComponentState.Created);

			_machine.Configure(RemoteComponentState.Created)
					.OnEntry(OnCreated)
					.Permit(RemoteComponentEvent.Ready, RemoteComponentState.Stopped);

			_machine.Configure(RemoteComponentState.Stopped)
			        .OnEntry(OnStopped)
			        .Permit(RemoteComponentEvent.Join, RemoteComponentState.Joining);

			_machine.Configure(RemoteComponentState.Joining)
			        .OnEntry(OnJoining)
			        .Permit(RemoteComponentEvent.Joined, RemoteComponentState.Joined)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored);

			_machine.Configure(RemoteComponentState.Joined)
			        .OnEntry(OnJoined)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
			        .Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped);
			
			_errorTrigger = _machine.SetTriggerParameters<Exception>(RemoteComponentEvent.ToError);

			_machine.Configure(RemoteComponentState.Errored)
					.OnEntryFrom(_errorTrigger, OnError)
					.OnEntry(a => OnError(null))
					.Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped);
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
			_errorExecutor.OnError(); //TODO: MOVE NEXT LINE TO THE EXECUTOR
			throw new ermeXRemoteComponentException(ex);
		}


		private void OnJoined(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			throw new NotImplementedException();
		}

		private void OnJoining(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			throw new NotImplementedException();
		}

		private void OnStopped(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			throw new NotImplementedException();
		}

		private void OnCreated(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			throw new NotImplementedException();
		}

		private void OnPrelive(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			throw new NotImplementedException();
		}


		public void Create()
		{
			throw new NotImplementedException();
		}

		public void Join()
		{
			throw new NotImplementedException();
		}

		public void Joined()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public bool IsErrored()
		{
			throw new NotImplementedException();
		}

		public bool IsStopped()
		{
			throw new NotImplementedException();
		}

		public bool IsRunning()
		{
			throw new NotImplementedException();
		}
	}
}
