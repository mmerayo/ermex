using System;
using Ninject;

namespace ermeX.Logging
{
	internal static class LogManager
	{
		[Obsolete()]
		public static ILogger GetLogger<TType>()
		{
			return GetLogger<TType>(Guid.Empty, LogComponent.Undefined);
		}

		[Obsolete()]
		public static ILogger GetLogger(Type type)
		{
			return GetLogger(type, Guid.Empty, LogComponent.Undefined);
		}

		public static ILogger GetLogger<TType>(Guid componentId, LogComponent logComponent)
		{
			return GetLogger(typeof (TType), componentId, logComponent);
		}

		public static ILogger GetLogger(Type type, Guid componentId, LogComponent logComponent)
		{
			return new Logger(type, componentId, logComponent);
		}

		public static ILogger GetLogger<TType>(LogComponent logComponent)
		{
			return GetLogger<TType>(Guid.Empty, logComponent);
		}

		public static ILogger GetLogger<TType>(Guid componentId)
		{
			return GetLogger<TType>(componentId, LogComponent.Undefined);
		}
	}
}