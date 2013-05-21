using Common.Logging;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.ComponentServices.LocalComponent
{
	internal sealed class LocalComponent : ILocalComponent
	{
		private static readonly ILog Logger = LogManager.GetLogger<LocalComponent>();
		private readonly LocalComponentStateMachine _stateMachine;

		public LocalComponent(LocalComponentStateMachine stateMachine)
		{
			_stateMachine = stateMachine;
		}

		public void Start()
		{
			Logger.Debug("Start");
			_stateMachine.Start();
		}
	}
}