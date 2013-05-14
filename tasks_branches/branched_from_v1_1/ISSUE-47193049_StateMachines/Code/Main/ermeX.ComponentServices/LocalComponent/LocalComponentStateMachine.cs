using Common.Logging;
using ermeX.ComponentServices.ComponentSetup;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponentStateMachine
	{
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponentStateMachine>();

		private enum MachineEvent
		{
			Start,
			SubscribeToMessages,
			PublishServices,
			Run,
			Stop,
			Reset,
			ToError
		}

		private enum MachineState
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
	}
}