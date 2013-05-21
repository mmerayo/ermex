using Common.Logging;
using ermeX.ComponentServices.LocalComponent.Commands;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponentStateMachineHandler : ILocalStateMachinePayloader
	{
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponentStateMachineHandler>();

		private readonly IStartablesStarterStepExecutor _startablesStarter;
		private readonly ISubscribeToMessagesStepExecutor _messagesSubscriber;
		private readonly IPublishServicesStepExecutor _servicesPublisher;
		private readonly IResetStepExecutor _resetExecutor;
		private readonly IStopComponentStepExecutor _stopExecutor;
		private readonly IRunStepExecutor _runExecutor;
		private readonly IErrorStepExecutor _errorExecutor;

		public LocalComponentStateMachineHandler(
			IStartablesStarterStepExecutor startablesStarter,
			ISubscribeToMessagesStepExecutor messagesSubscriber,
			IPublishServicesStepExecutor servicesPublisher,
			IResetStepExecutor resetExecutor,
			IStopComponentStepExecutor stopExecutor,
			IRunStepExecutor runExecutor,
			IErrorStepExecutor errorExecutor)
		{
			_startablesStarter = startablesStarter;
			_messagesSubscriber = messagesSubscriber;
			_servicesPublisher = servicesPublisher;
			_resetExecutor = resetExecutor;
			_stopExecutor = stopExecutor;
			_runExecutor = runExecutor;
			_errorExecutor = errorExecutor;
		}

		public void Reset()
		{
			Logger.Debug("Reset");

			_resetExecutor.Reset();
		}

		public void Stop()
		{
			Logger.Debug("Stop");

			_stopExecutor.Stop();
		}

		public void Run()
		{
			Logger.Debug("Run");

			_runExecutor.Run();
		}

		public void PublishServices()
		{
			Logger.Debug("PublishServices");

			_servicesPublisher.Publish();
		}

		public void SubscribeToMessages()
		{
			Logger.Debug("SubscribeToMessages");

			_messagesSubscriber.Subscribe();
		}

		public void OnStart()
		{
			Logger.Debug("OnStart");

			_startablesStarter.DoStart();

		}

		public void Error()
		{
			Logger.Debug("Error");

			_errorExecutor.OnError();
		}
	}
}