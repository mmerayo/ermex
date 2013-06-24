using System;
using Common.Logging;

namespace ermeX.Logging
{
	internal interface ILogger
	{
		void Trace(object message);


		void Trace(object message, Exception exception);


		void TraceFormat(string format, params object[] args);


		void TraceFormat(string format, Exception exception, params object[] args);


		void TraceFormat(IFormatProvider formatProvider, string format, params object[] args);


		void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);


		void Trace(Action<FormatMessageHandler> formatMessageCallback);

		void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

		void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Debug(object message);

		void Debug(object message, Exception exception);
		void DebugFormat(string format, params object[] args);


		void DebugFormat(string format, Exception exception, params object[] args);


		void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);

		void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);


		void Debug(Action<FormatMessageHandler> formatMessageCallback);


		void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);


		void Info(object message);

		void Info(object message, Exception exception);

		void InfoFormat(string format, params object[] args);

		void InfoFormat(string format, Exception exception, params object[] args);

		void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);

		void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);

		void Info(Action<FormatMessageHandler> formatMessageCallback);

		void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

		void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Warn(object message);

		void Warn(object message, Exception exception);

		void WarnFormat(string format, params object[] args);

		void WarnFormat(string format, Exception exception, params object[] args);

		void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);

		void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);

		void Warn(Action<FormatMessageHandler> formatMessageCallback);

		void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

		void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Error(object message);

		void Error(object message, Exception exception);

		void ErrorFormat(string format, params object[] args);

		void ErrorFormat(string format, Exception exception, params object[] args);

		void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);

		void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);

		void Error(Action<FormatMessageHandler> formatMessageCallback);

		void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

		void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Fatal(object message);

		void Fatal(object message, Exception exception);

		void FatalFormat(string format, params object[] args);

		void FatalFormat(string format, Exception exception, params object[] args);

		void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);

		void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);

		void Fatal(Action<FormatMessageHandler> formatMessageCallback);

		void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback);

		void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception);

		 bool IsTraceEnabled { get;  }
		 bool IsDebugEnabled { get;  }
		 bool IsErrorEnabled { get;  }
		 bool IsFatalEnabled { get;  }
		 bool IsInfoEnabled { get;  }
		 bool IsWarnEnabled { get;  }
	}
}