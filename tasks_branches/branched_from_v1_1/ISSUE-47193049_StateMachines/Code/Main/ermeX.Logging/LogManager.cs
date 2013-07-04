using System;
using Ninject;

namespace ermeX.Logging
{
	internal sealed class LogManager : ILogManager
	{
		private readonly Guid _componentId;
		private readonly LogComponent _logComponent;

		[Inject]
		public LogManager(Guid componentId,LogComponent logComponent)
		{
			_componentId = componentId;
			_logComponent = logComponent;
		}

		public ILogger GetLogger<TType>()
		{
			return GetLogger(typeof(TType));
		}

		public ILogger GetLogger(Type type)
		{
			return new Logger(type, _componentId, _logComponent);
		}

		public static ILogger GetNonQualifiedLogger<TType>()
		{
			return new Logger(typeof(TType));
		}

		public static ILogger GetNonQualifiedLogger(Type type)
		{
			return new Logger(type);
		}
	}
	
	
}