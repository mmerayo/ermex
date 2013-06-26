using System;

using Ninject;
using Stateless;
using ermeX.ComponentServices.ComponentSetup;
using ermeX.ComponentServices.Interfaces.LocalComponent;
using ermeX.ComponentServices.Interfaces.LocalComponent.Commands;
using ermeX.ComponentServices.LocalComponent.Commands;
using ermeX.ConfigurationManagement.IoC;
using ermeX.Exceptions;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponentStateMachine : ILocalComponentStateMachine
	{

		private enum LocalComponentEvent
		{
			Start,
			SubscribeToMessages,
			PublishServices,
			Run,
			Stop,
			Reset,
			ToError
		}

		private enum LocalComponentState
		{
			Stopped = 0,
			Starting,
			SubscribingMessageHandlers,
			PublishingServices,
			Running,
			Stopping,
			Resetting,
			Error
		}

		private static readonly ILogger Logger = LogManager.GetLogger<LocalComponentStateMachine>();

		private readonly StateMachine<LocalComponentState, LocalComponentEvent> _machine =
			new StateMachine<LocalComponentState, LocalComponentEvent>(LocalComponentState.Stopped);

		private StateMachine<LocalComponentState, LocalComponentEvent>.TriggerWithParameters<Exception> _errorTrigger;

		private IOnStartStepExecutor _startablesStarter;
		private IOnSubscribeToMessagesStepExecutor _messagesSubscriber;
		private IOnPublishServicesStepExecutor _servicesPublisher;
		private IOnResetStepExecutor _onResetExecutor;
		private IOnStopStepExecutor _onStopExecutor;
		private IOnRunStepExecutor _onRunExecutor;
		private IOnErrorStepExecutor _errorExecutor;

		[Inject]
		public LocalComponentStateMachine()
		{
			DefineStateMachineTransitions();
		}

		private void DefineStateMachineTransitions()
		{
			Logger.Debug("DefineStateMachineTransitions");
			_machine.Configure(LocalComponentState.Stopped)
			        .OnEntry(OnStopped)
			        .Permit(LocalComponentEvent.Start, LocalComponentState.Starting);

			_machine.Configure(LocalComponentState.Starting)
			        .OnEntry(OnStarting)
			        .Permit(LocalComponentEvent.SubscribeToMessages, LocalComponentState.SubscribingMessageHandlers)
			        .Permit(LocalComponentEvent.ToError, LocalComponentState.Error);

			_machine.Configure(LocalComponentState.SubscribingMessageHandlers)
			        .OnEntry(OnSubscribingMessageHandlers)
			        .Permit(LocalComponentEvent.PublishServices, LocalComponentState.PublishingServices)
			        .Permit(LocalComponentEvent.ToError, LocalComponentState.Error);

			_machine.Configure(LocalComponentState.PublishingServices)
			        .OnEntry(OnPublishingServices)
			        .Permit(LocalComponentEvent.Run, LocalComponentState.Running)
			        .Permit(LocalComponentEvent.ToError, LocalComponentState.Error);

			_machine.Configure(LocalComponentState.Running)
			        .OnEntry(OnRunning)
			        .Permit(LocalComponentEvent.Stop, LocalComponentState.Stopping)
			        .Permit(LocalComponentEvent.ToError, LocalComponentState.Error);

			_machine.Configure(LocalComponentState.Stopping)
			        .OnEntry(OnStopping)
			        .Permit(LocalComponentEvent.Reset, LocalComponentState.Resetting)
			        .Permit(LocalComponentEvent.ToError, LocalComponentState.Error);

			_machine.Configure(LocalComponentState.Resetting)
			        .OnEntry(OnResetting)
			        .Permit(LocalComponentEvent.Stop, LocalComponentState.Stopped)
			        .Permit(LocalComponentEvent.ToError, LocalComponentState.Error);


			_errorTrigger = _machine.SetTriggerParameters<Exception>(LocalComponentEvent.ToError);

			_machine.Configure(LocalComponentState.Error)
			        .OnEntryFrom(_errorTrigger, OnError)
			        .OnEntry(a => OnError(null));

		}

		private void RefreshInjections()
		{
			_startablesStarter = IoCManager.Kernel.Get<IOnStartStepExecutor>();
			_messagesSubscriber = IoCManager.Kernel.Get<IOnSubscribeToMessagesStepExecutor>();
			_servicesPublisher = IoCManager.Kernel.Get<IOnPublishServicesStepExecutor>();
			_onResetExecutor = IoCManager.Kernel.Get<IOnResetStepExecutor>();
			_onStopExecutor = IoCManager.Kernel.Get<IOnStopStepExecutor>();
			_onRunExecutor = IoCManager.Kernel.Get<IOnRunStepExecutor>();
			_errorExecutor = IoCManager.Kernel.Get<IOnErrorStepExecutor>();
		}

		private void FireError(Exception ex)
		{
			Logger.WarnFormat("FireError-{0}", ex.ToString());
			if (IsErrored())
				throw ex;
			_machine.Fire(_errorTrigger, ex);
		}


		private void TryFire(LocalComponentEvent e)
		{
			Logger.DebugFormat("TryFire - {0}", e);
			if (_machine == null)
				throw new ApplicationException("FATAL");
			if (!_machine.CanFire(e))
				throw new InvalidOperationException(string.Format("Cannot transit from:{0} with trigger:{1}", _machine.State, e));
			if (e == LocalComponentEvent.ToError)
				throw new InvalidOperationException("Use FireError");
			_machine.Fire(e);
		}

		private void OnError(Exception ex)
		{
			if (ex == null) throw new ArgumentNullException("ex");
			Logger.ErrorFormat("OnError - {0}", ex.ToString());
			_errorExecutor.OnError(); //TODO: MOVE NEXT LINE TO THE EXECUTOR
			throw new ermeXLocalComponentException(ex);
		}

		public bool IsErrored()
		{
			return _machine.State == LocalComponentState.Error;
		}

		public void Start()
		{
			TryFire(LocalComponentEvent.Start);
		}

		public void Stop()
		{
			TryFire(LocalComponentEvent.Stop);

		}


		private void OnResetting(StateMachine<LocalComponentState, LocalComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnResetting-{0}", obj.Trigger);
			try
			{
				_onResetExecutor.Reset();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(LocalComponentEvent.Stop);
		}

		private void OnStopping(StateMachine<LocalComponentState, LocalComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnStopping-{0}", obj.Trigger);
			try
			{
				_onStopExecutor.Stop();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(LocalComponentEvent.Reset);
		}

		private void OnRunning(StateMachine<LocalComponentState, LocalComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnStopping-{0}", obj.Trigger);
			try
			{
				_onRunExecutor.Run();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		private void OnPublishingServices(StateMachine<LocalComponentState, LocalComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnStopping-{0}", obj.Trigger);
			try
			{
				_servicesPublisher.Publish();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(LocalComponentEvent.Run);
		}

		private void OnSubscribingMessageHandlers(StateMachine<LocalComponentState, LocalComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnStopping-{0}", obj.Trigger);
			try
			{
				_messagesSubscriber.Subscribe();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(LocalComponentEvent.PublishServices);
		}

		private void OnStarting(StateMachine<LocalComponentState, LocalComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnStarting-{0}", obj.Trigger);
			try
			{
				RefreshInjections();
				_startablesStarter.DoStart();
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
			TryFire(LocalComponentEvent.SubscribeToMessages);
		}

		

		private void OnStopped(StateMachine<LocalComponentState, LocalComponentEvent>.Transition obj)
		{
			Logger.DebugFormat("OnStopped-{0}", obj.Trigger);
			try
			{
				//TODO:
			}
			catch (Exception ex)
			{
				FireError(ex);
			}
		}

		public bool IsStopped()
		{
			return _machine.State == LocalComponentState.Stopped;
		}

		public bool IsRunning()
		{
			return _machine.State == LocalComponentState.Running;
		}
	}
}