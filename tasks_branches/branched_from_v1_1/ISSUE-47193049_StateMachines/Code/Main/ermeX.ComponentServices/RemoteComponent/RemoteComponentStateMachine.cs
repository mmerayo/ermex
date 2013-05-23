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
		private readonly IOnRequestingSubscriptionsStepExecutor _subscriptionsRequester;
		private readonly IOnSubscriptionsReceivedStepExecutor _subscriptionsReceivedHandler;
		private readonly IOnRequestingServicesStepExecutor _servicesRequester;
		private readonly IOnServicesReceivedStepExecutor _servicesReceivedHandler;

		private enum RemoteComponentEvent
		{
			Create=0,
			Ready,
			Join,
			Joined,
			ToError,
			Stop,
			//sub state machine
			RequestServices,
			ServicesReceived,
			RequestSubscriptions,
			SubscriptionsReceived,
			UnAvailable
		}
		private enum RemoteComponentState
		{
			Prelive=0,
			Creating,
			Stopped,
			Joining,
			Running,
			Errored,
			//sub state machine
			RequestingServices,
			ServicesReceived,
			RequestingSubscriptions,
			SubscriptionsReceived
		}
		private static readonly ILog Logger = LogManager.GetLogger<RemoteComponentStateMachine>();

		private readonly StateMachine<RemoteComponentState, RemoteComponentEvent> _machine =
			new StateMachine<RemoteComponentState, RemoteComponentEvent>(RemoteComponentState.Prelive);

		private StateMachine<RemoteComponentState, RemoteComponentEvent>.TriggerWithParameters<Exception> _errorTrigger;
		private bool _wasCreated=false;


		[Inject]
		public RemoteComponentStateMachine(IOnPreliveExecutor preliveExecutor,
			IOnCreatingStepExecutor creatingExecutor,
			IOnStoppedStepExecutor stoppedStepExecutor,
			IOnJoiningStepExecutor joiningStepExecutor,
			IOnRunningStepExecutor runningStepExecutor,
			IOnErrorStepExecutor errorStepExecutor,
			IOnRequestingSubscriptionsStepExecutor subscriptionsRequester,
			IOnSubscriptionsReceivedStepExecutor subscriptionsReceivedHandler,
			IOnRequestingServicesStepExecutor servicesRequester,
			IOnServicesReceivedStepExecutor servicesReceivedHandler)
		{
			_preliveExecutor = preliveExecutor;
			_creatingExecutor = creatingExecutor;
			_stoppedStepExecutor = stoppedStepExecutor;
			_joiningStepExecutor = joiningStepExecutor;
			_runningStepExecutor = runningStepExecutor;
			_errorStepExecutor = errorStepExecutor;
			_subscriptionsRequester = subscriptionsRequester;
			_subscriptionsReceivedHandler = subscriptionsReceivedHandler;
			_servicesRequester = servicesRequester;
			_servicesReceivedHandler = servicesReceivedHandler;
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
					.Permit(RemoteComponentEvent.UnAvailable,RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored);

			_machine.Configure(RemoteComponentState.Running)
			        .OnEntry(OnRunning)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
			        .Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.RequestServices, RemoteComponentState.RequestingServices);

			_errorTrigger = _machine.SetTriggerParameters<Exception>(RemoteComponentEvent.ToError);

			_machine.Configure(RemoteComponentState.Errored)
			        .OnEntryFrom(_errorTrigger, OnError)
			        .OnEntry(a => OnError(null));

			DefineRunningSubStateMachine();
		}

		private void DefineRunningSubStateMachine()
		{
			_machine.Configure(RemoteComponentState.RequestingServices)
			        .SubstateOf(RemoteComponentState.Running)
			        .OnEntry(OnRequestingServices)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
			        .Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.ServicesReceived, RemoteComponentState.ServicesReceived);

			_machine.Configure(RemoteComponentState.ServicesReceived)
			        .SubstateOf(RemoteComponentState.Running)
			        .OnEntry(OnServicesReceived)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
			        .Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.RequestSubscriptions, RemoteComponentState.RequestingSubscriptions);

			_machine.Configure(RemoteComponentState.RequestingSubscriptions)
			        .SubstateOf(RemoteComponentState.Running)
			        .OnEntry(OnRequestingSubscriptions)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
			        .Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.SubscriptionsReceived, RemoteComponentState.SubscriptionsReceived);

			_machine.Configure(RemoteComponentState.SubscriptionsReceived)
			        .SubstateOf(RemoteComponentState.Running)
			        .OnEntry(OnSubscriptionsReceived)
			        .Permit(RemoteComponentEvent.ToError, RemoteComponentState.Errored)
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
			_errorStepExecutor.OnError(); //TODO: MOVE NEXT LINE TO THE EXECUTOR
			throw new ermeXRemoteComponentException(ex);
		}

		private void OnSubscriptionsReceived(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnSubscriptionsReceived-{0}", obj.Trigger);
			try
			{
				_subscriptionsReceivedHandler.SubscriptionsReceived();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			
		}

		private void OnRequestingSubscriptions(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnRequestingSubscriptions-{0}", obj.Trigger);
			try
			{
				_subscriptionsRequester.Request();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnServicesReceived(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnServicesReceived-{0}", obj.Trigger);
			try
			{
				_servicesReceivedHandler.ServicesReceived();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(RemoteComponentEvent.RequestSubscriptions);
		}

		private void OnRequestingServices(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnRequestingServices-{0}", obj.Trigger);
			try
			{
				_servicesRequester.Request();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
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
			TryFire(RemoteComponentEvent.RequestServices);
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
			_wasCreated = true;
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
			return _machine.IsInState(RemoteComponentState.Running);
		}

		public bool WasCreated()
		{
			return _wasCreated;
		}

		public bool IsJoining()
		{
			return _machine.State == RemoteComponentState.Joining;
		}

		public bool IsRequestingServices()
		{
			return _machine.State == RemoteComponentState.RequestingServices;
		}

		public void SubscriptionsReceived()
		{
			//TODO: ASK PARAM AND POPULATE CONTEXT
			TryFire(RemoteComponentEvent.SubscriptionsReceived);
		}

		public void ServicesReceived()
		{
			//TODO: ASK PARAM AND POPULATE CONTEXT
			TryFire(RemoteComponentEvent.ServicesReceived);
		}
	}
}
