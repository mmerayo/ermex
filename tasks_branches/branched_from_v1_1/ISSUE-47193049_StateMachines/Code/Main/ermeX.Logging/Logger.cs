using System;
using Common.Logging;
using Common.Logging.Simple;


namespace ermeX.Logging
{
	internal sealed class Logger : ILogger
	{
		private readonly ILog _innerLogger;
		private readonly string _logPrefix;
		
		public Logger(Type type, Guid componentId, LogComponent logComponent)
		{
#if DEBUG //TODO: MOVE TO THE TESTFIXTURESETUPS
			if (Common.Logging.LogManager.Adapter is NoOpLoggerFactoryAdapter)
				Common.Logging.LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(LogLevel.All, true, true, true, "yyyy/MM/dd HH:mm:ss:fff");
#endif
			_logPrefix = string.Format(" - AppDomainId: {0} - ComponentId: {1} - SubComponent: {2}", AppDomain.CurrentDomain.Id,componentId==Guid.Empty?"NOT SET":componentId.ToString(), logComponent.ToString());

			_innerLogger = Common.Logging.LogManager.GetLogger(type);
		}

		public Logger(Type type):this(type,Guid.Empty,LogComponent.Undefined) { }

		private Action<FormatMessageHandler> GetMessageCallback(string message)
		{
			return m => m("{0} - ThreadId: {1} - {2}", _logPrefix, System.Threading.Thread.CurrentThread.ManagedThreadId, message);
		}

		private Action<FormatMessageHandler> GetMessageCallback(Action<FormatMessageHandler> callback)
		{
			return m => m(string.Format("{0} - {1}", _logPrefix, callback));
		}

		private Action<FormatMessageHandler> GetMessageCallback(string format, params object[] args)
		{
			return m => m(string.Format("{0} - {1}", _logPrefix, string.Format(format, args)));
		}

		public void Trace(object message)
		{
			_innerLogger.Trace(GetMessageCallback(message.ToString()));
		}

		public void Trace(object message, Exception exception)
		{
			_innerLogger.Trace(GetMessageCallback(message.ToString()), exception);
		}

		public void TraceFormat(string format, params object[] args)
		{
			_innerLogger.Trace(GetMessageCallback(format,args));
		}

		public void Trace(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Trace(GetMessageCallback(callback));
		}

		public void Trace(Action<FormatMessageHandler> callback, Exception exception)
		{
			_innerLogger.Trace(GetMessageCallback(callback),exception);
		}

		public void Debug(object message)
		{
			_innerLogger.Debug(GetMessageCallback(message.ToString()));
		}

		public void Debug(object message, Exception exception)
		{
			_innerLogger.Debug(GetMessageCallback(message.ToString()), exception);
		}

		public void DebugFormat(string format, params object[] args)
		{
			_innerLogger.Debug(GetMessageCallback(format, args));
		}

		public void Debug(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Debug(GetMessageCallback(callback));
		}

		public void Debug(Action<FormatMessageHandler> callback, Exception exception)
		{
			_innerLogger.Debug(GetMessageCallback(callback), exception);
		}

		public void Info(object message)
		{
			_innerLogger.Info(GetMessageCallback(message.ToString()));
		}

		public void Info(object message, Exception exception)
		{
			_innerLogger.Info(GetMessageCallback(message.ToString()), exception);
		}

		public void InfoFormat(string format, params object[] args)
		{
			_innerLogger.Info(GetMessageCallback(format, args));
		}

		public void Info(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Info(GetMessageCallback(callback));
		}

		public void Info(Action<FormatMessageHandler> callback, Exception exception)
		{
			_innerLogger.Info(GetMessageCallback(callback), exception);
		}

		public void Warn(object message)
		{
			_innerLogger.Warn(GetMessageCallback(message.ToString()));
		}

		public void Warn(object message, Exception exception)
		{
			_innerLogger.Warn(GetMessageCallback(message.ToString()), exception);
		}

		public void WarnFormat(string format, params object[] args)
		{
			_innerLogger.Warn(GetMessageCallback(format, args));
		}

		public void Warn(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Warn(GetMessageCallback(callback));
		}

		public void Warn(Action<FormatMessageHandler> callback, Exception exception)
		{
			_innerLogger.Warn(GetMessageCallback(callback), exception);
		}

		public void Error(object message)
		{
			_innerLogger.Error(GetMessageCallback(message.ToString()));
		}

		public void Error(object message, Exception exception)
		{
			_innerLogger.Error(GetMessageCallback(message.ToString()), exception);
		}

		public void ErrorFormat(string format, params object[] args)
		{
			_innerLogger.Error(GetMessageCallback(format, args));
		}

		public void Error(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Error(GetMessageCallback(callback));
		}

		public void Error(Action<FormatMessageHandler> callback, Exception exception)
		{
			_innerLogger.Error(GetMessageCallback(callback), exception);
		}

		public void Fatal(object message)
		{
			_innerLogger.Fatal(GetMessageCallback(message.ToString()));
		}

		public void Fatal(object message, Exception exception)
		{
			_innerLogger.Fatal(GetMessageCallback(message.ToString()), exception);
		}

		public void FatalFormat(string format, params object[] args)
		{
			_innerLogger.Fatal(GetMessageCallback(format, args));
		}

		public void Fatal(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Fatal(GetMessageCallback(callback));
		}

		public void Fatal(Action<FormatMessageHandler> callback, Exception exception)
		{
			_innerLogger.Fatal(GetMessageCallback(callback), exception);
		}

		public bool IsTraceEnabled
		{
			get { return _innerLogger.IsTraceEnabled; }
		}

		public bool IsDebugEnabled
		{
			get { return _innerLogger.IsDebugEnabled; }
		}

		public bool IsErrorEnabled
		{
			get { return _innerLogger.IsErrorEnabled; }
		}

		public bool IsFatalEnabled
		{
			get { return _innerLogger.IsFatalEnabled; }
		}

		public bool IsInfoEnabled
		{
			get { return _innerLogger.IsInfoEnabled; }
		}

		public bool IsWarnEnabled
		{
			get { return _innerLogger.IsWarnEnabled; }
		}
	}
}