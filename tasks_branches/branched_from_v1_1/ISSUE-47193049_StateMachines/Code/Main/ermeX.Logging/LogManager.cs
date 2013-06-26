using System;

namespace ermeX.Logging
{
	internal sealed class LogManager : ILogManager
	{
		private readonly Guid _componentId;
		private readonly LogComponent _logComponent;
		//TODO: INJECT
		public LogManager(Guid componentId,LogComponent logComponent)
		{
			_componentId = componentId;
			_logComponent = logComponent;
		}

		public ILogger GetLogger<TType>()
		{
			return new Logger(typeof(TType),_componentId,_logComponent);
		}

		public static ILogger GetNonQualifiedLogger<TType>()
		{
			return new Logger(typeof(TType));
		}
	}
}