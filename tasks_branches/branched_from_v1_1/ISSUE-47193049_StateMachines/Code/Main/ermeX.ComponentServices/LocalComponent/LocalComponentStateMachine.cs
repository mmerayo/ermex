using System;
using Common.Logging;
using Stateless;
using ermeX.ComponentServices.ComponentSetup;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed partial class LocalComponent
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

		private readonly StateMachine<LocalComponentState, LocalComponentEvent> _machine =
			new StateMachine<LocalComponentState, LocalComponentEvent>(LocalComponentState.Stopped);

		private StateMachine<LocalComponentState, LocalComponentEvent>.TriggerWithParameters<Exception> _errorTrigger;

		private void DefineStateMachineTransitions()
		{
			Logger.Debug("DefineStateMachineTransitions");
			_machine.Configure(LocalComponentState.Stopped)
					.OnEntry(OnStopped)
					.Permit(SetupEvent.Inject, SetupProcessState.InjectingServices)
					.Permit(SetupEvent.Error, SetupProcessState.Error);
		}
	}
}