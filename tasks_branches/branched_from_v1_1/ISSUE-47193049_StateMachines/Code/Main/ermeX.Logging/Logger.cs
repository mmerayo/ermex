using System;
using Common.Logging;


namespace ermeX.Logging
{
	internal sealed class Logger : ILogger
	{

		private ILog _innerLogger;
		private readonly string _logPrefix;


		public Logger(Type type, Guid componentId, LogComponent logComponent)
		{
			_logPrefix = string.Format(" - AppDomainId: {0} - ComponentId: {1} - SubComponent: {2}", AppDomain.CurrentDomain.Id,componentId, logComponent.ToString());

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


		public void Trace(object message)
		{
			_innerLogger.Trace(GetMessageCallback(message.ToString()));
		}

		public void Trace(object message, Exception exception)
		{
			_innerLogger.Trace(GetMessageCallback(message.ToString()), exception);
		}

		public void Trace(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Trace(GetMessageCallback(callback));
		}

		public void Debug(object message)
		{
			_innerLogger.Debug(GetMessageCallback(message.ToString()));
		}

		public void Debug(object message, Exception exception)
		{
			_innerLogger.Debug(GetMessageCallback(message.ToString()), exception);
		}

		public void Debug(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Debug(GetMessageCallback(callback));
		}

		public void Info(object message)
		{
			_innerLogger.Info(GetMessageCallback(message.ToString()));
		}

		public void Info(object message, Exception exception)
		{
			_innerLogger.Info(GetMessageCallback(message.ToString()), exception);
		}

		public void Info(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Info(GetMessageCallback(callback));
		}

		public void Warn(object message)
		{
			_innerLogger.Warn(GetMessageCallback(message.ToString()));
		}

		public void Warn(object message, Exception exception)
		{
			_innerLogger.Warn(GetMessageCallback(message.ToString()), exception);
		}

		public void Warn(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Warn(GetMessageCallback(callback));
		}

		public void Error(object message)
		{
			_innerLogger.Error(GetMessageCallback(message.ToString()));
		}

		public void Error(object message, Exception exception)
		{
			_innerLogger.Error(GetMessageCallback(message.ToString()), exception);
		}

		public void Error(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Error(GetMessageCallback(callback));
		}

		public void Fatal(object message)
		{
			_innerLogger.Fatal(GetMessageCallback(message.ToString()));
		}

		public void Fatal(object message, Exception exception)
		{
			_innerLogger.Fatal(GetMessageCallback(message.ToString()), exception);
		}

		public void Fatal(Action<FormatMessageHandler> callback)
		{
			_innerLogger.Fatal(GetMessageCallback(callback));
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