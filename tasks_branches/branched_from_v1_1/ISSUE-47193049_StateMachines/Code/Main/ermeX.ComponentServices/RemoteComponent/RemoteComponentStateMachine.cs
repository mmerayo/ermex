using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Common.Logging;
using Ninject;
using Stateless;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ComponentServices.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.IoC;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.RemoteComponent
{
	internal sealed class RemoteComponentStateMachine:IRemoteComponentStateMachine
	{
		private readonly IRemoteComponentStateMachineContext _context;
		private IOnPreliveExecutor _preliveExecutor;
		private IOnCreatingStepExecutor _creatingExecutor;
		private IOnStoppedStepExecutor _stoppedStepExecutor;
		private IOnJoiningStepExecutor _joiningStepExecutor;
		private IOnRunningStepExecutor _runningStepExecutor;
		private IOnErrorStepExecutor _errorStepExecutor;
		private IOnRequestingSubscriptionsStepExecutor _subscriptionsRequester;
		private IOnSubscriptionsReceivedStepExecutor _subscriptionsReceivedHandler;
		private IOnRequestingServicesStepExecutor _servicesRequester;
		private IOnServicesReceivedStepExecutor _servicesReceivedHandler;

		private enum RemoteComponentEvent
		{
			ToPrelive=0,
			Create,
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
			NotStarted=0,
			Prelive,
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

		public IRemoteComponentStateMachineContext Context
		{
			get { return _context; }
		}

		private static readonly ILog Logger = LogManager.GetLogger<RemoteComponentStateMachine>();

		private readonly StateMachine<RemoteComponentState, RemoteComponentEvent> _machine =
			new StateMachine<RemoteComponentState, RemoteComponentEvent>(RemoteComponentState.NotStarted);

		private StateMachine<RemoteComponentState, RemoteComponentEvent>.TriggerWithParameters<Exception> _errorTrigger;
		private bool _wasCreated=false;


		[Inject]
		public RemoteComponentStateMachine(IRemoteComponentStateMachineContext context)
		{
			_context = context;
			_context.StateMachine = this;
			
			DefineStateMachineTransitions();
			TryFire(RemoteComponentEvent.ToPrelive);
		}

		private void RefreshInjections()
		{
			_preliveExecutor = IoCManager.Kernel.Get<IOnPreliveExecutor>();
			_creatingExecutor = IoCManager.Kernel.Get<IOnCreatingStepExecutor>();
			_stoppedStepExecutor = IoCManager.Kernel.Get<IOnStoppedStepExecutor>();
			_joiningStepExecutor = IoCManager.Kernel.Get<IOnJoiningStepExecutor>();
			_runningStepExecutor = IoCManager.Kernel.Get<IOnRunningStepExecutor>();
			_errorStepExecutor = IoCManager.Kernel.Get<IOnErrorStepExecutor>();
			_subscriptionsRequester = IoCManager.Kernel.Get<IOnRequestingSubscriptionsStepExecutor>();
			_subscriptionsReceivedHandler = IoCManager.Kernel.Get<IOnSubscriptionsReceivedStepExecutor>();
			_servicesRequester = IoCManager.Kernel.Get<IOnRequestingServicesStepExecutor>();
			_servicesReceivedHandler = IoCManager.Kernel.Get<IOnServicesReceivedStepExecutor>();

			Debug.Assert(_preliveExecutor!=null);
			Debug.Assert(_creatingExecutor != null);
			Debug.Assert(_stoppedStepExecutor != null);
			Debug.Assert(_joiningStepExecutor != null);
			Debug.Assert(_runningStepExecutor != null);
			Debug.Assert(_errorStepExecutor != null);
			Debug.Assert(_subscriptionsRequester != null);
			Debug.Assert(_subscriptionsReceivedHandler != null);
			Debug.Assert(_servicesRequester != null);
			Debug.Assert(_servicesReceivedHandler != null);
		}

		private void DefineStateMachineTransitions()
		{
			Logger.Debug("DefineStateMachineTransitions");
			_machine.Configure(RemoteComponentState.NotStarted)
					.Permit(RemoteComponentEvent.ToPrelive, RemoteComponentState.Prelive);

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
					.Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.UnAvailable, RemoteComponentState.Stopped);

			_machine.Configure(RemoteComponentState.Running)
			        .OnEntry(OnRunning)
			        .Permit(RemoteComponentEvent.UnAvailable, RemoteComponentState.Stopped)
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
					//.Permit(RemoteComponentEvent.UnAvailable, RemoteComponentState.Stopped)
					//.Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.ServicesReceived, RemoteComponentState.ServicesReceived);

			_machine.Configure(RemoteComponentState.ServicesReceived)
			        .SubstateOf(RemoteComponentState.Running)
			        .OnEntry(OnServicesReceived)
				//.Permit(RemoteComponentEvent.UnAvailable, RemoteComponentState.Stopped)
				//.Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.RequestSubscriptions, RemoteComponentState.RequestingSubscriptions);

			_machine.Configure(RemoteComponentState.RequestingSubscriptions)
			        .SubstateOf(RemoteComponentState.Running)
			        .OnEntry(OnRequestingSubscriptions)
				//.Permit(RemoteComponentEvent.UnAvailable, RemoteComponentState.Stopped)
				//.Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
			        .Permit(RemoteComponentEvent.SubscriptionsReceived, RemoteComponentState.SubscriptionsReceived);

			_machine.Configure(RemoteComponentState.SubscriptionsReceived)
			        .SubstateOf(RemoteComponentState.Running)
			        .OnEntry(OnSubscriptionsReceived);
			//.Permit(RemoteComponentEvent.UnAvailable, RemoteComponentState.Stopped)
			//.Permit(RemoteComponentEvent.Stop, RemoteComponentState.Stopped)
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
			Logger.ErrorFormat("OnError - {0}", ex.ToString());
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
				StopDueToUnAvailability(obj, ex);
			}
			
		}

		private void OnRequestingSubscriptions(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnRequestingSubscriptions-{0}", obj.Trigger);
			try
			{
				_subscriptionsRequester.Request(_context);
			}
			catch (Exception ex)
			{
				StopDueToUnAvailability(obj, ex);
			}
		}

		private void OnServicesReceived(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnServicesReceived-{0}", obj.Trigger);
			try
			{
				_servicesReceivedHandler.ServicesReceived(_context);
			}
			catch (Exception ex)
			{
				StopDueToUnAvailability(obj, ex);
				return;
			}
			TryFire(RemoteComponentEvent.RequestSubscriptions);
		}

		private void OnRequestingServices(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnRequestingServices-{0}", obj.Trigger);
			try
			{
				_servicesRequester.Request(_context);
			}
			catch (Exception ex)
			{
				StopDueToUnAvailability(obj, ex);
			}
		}

	

		private void OnRunning(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnJoined-{0}", obj.Trigger);
			try
			{
				_runningStepExecutor.OnRunning(_context);
			}
			catch (Exception ex)
			{
				StopDueToUnAvailability(obj, ex);
				return;
			}
			TryFire(RemoteComponentEvent.RequestServices);
		}

		private void OnJoining(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnJoining-{0}", obj.Trigger);
			try
			{
				_joiningStepExecutor.Join(_context);
			}
			catch (Exception ex)
			{
				StopDueToUnAvailability(obj, ex);
			}
			Joined();
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
			Logger.DebugFormat("OnCreating-{0} - Component:{1}", obj.Trigger,_context.ComponentId);
			try
			{
				_creatingExecutor.Create(_context);
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
				RefreshInjections();
				_preliveExecutor.OnPrelive();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		public void Create (Guid componentId, IPAddress ipAddress, ushort port)
		{
			_context.ComponentId = componentId;
			_context.IpAddress = ipAddress;
			_context.Port = port;

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

		public bool IsRequestingSubscriptions()
		{
			return _machine.IsInState(RemoteComponentState.RequestingSubscriptions);
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

		

		private void StopDueToUnAvailability(StateMachine<RemoteComponentState, RemoteComponentEvent>.Transition obj, Exception ex)
		{
			Logger.InfoFormat("Could not transit to {0} due to {1}", obj.Destination, ex.ToString());
			TryFire(RemoteComponentEvent.UnAvailable);
		}
	}
}
