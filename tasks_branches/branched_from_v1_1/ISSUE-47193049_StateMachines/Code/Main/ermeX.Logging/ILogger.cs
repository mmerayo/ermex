using System;
using Common.Logging;


namespace ermeX.Logging
{
	internal interface ILogger
	{
		void Trace(object message);
		void Trace(object message, Exception exception);
		void Trace(Action<FormatMessageHandler> callback);

		void Debug(object message);
		void Debug(object message, Exception exception);
		void Debug(Action<FormatMessageHandler> callback);

		void Info(object message);
		void Info(object message, Exception exception);
		void Info(Action<FormatMessageHandler> callback);

		void Warn(object message);
		void Warn(object message, Exception exception);
		void Warn(Action<FormatMessageHandler> callback);

		void Error(object message);
		void Error(object message, Exception exception);
		void Error(Action<FormatMessageHandler> callback);
		
		void Fatal(object message);
		void Fatal(object message, Exception exception);
		void Fatal(Action<FormatMessageHandler> callback);

		 bool IsTraceEnabled { get;  }
		 bool IsDebugEnabled { get;  }
		 bool IsErrorEnabled { get;  }
		 bool IsFatalEnabled { get;  }
		 bool IsInfoEnabled { get;  }
		 bool IsWarnEnabled { get;  }
	}
}