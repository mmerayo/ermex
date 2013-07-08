using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using ermeX.Logging;
using ermeX.Tests.Common.Reflection;
using FormatMessageHandler = ermeX.Logging.FormatMessageHandler;


namespace ermeX.Tests.Logging
{
	[TestFixture]
	public class LoggerTests
	{
		private class TestContext
		{
			Fixture _fixture;
			Mock<ILog> _innerLoggerMock;

			public TestContext()
			{
				_fixture = new Fixture();
				TheType = GetType();
				TheComponentId = Guid.NewGuid();
				TheLogComponent= _fixture.Create<LogComponent>();

				Sut= new Logger(TheType, TheComponentId, TheLogComponent);

				_innerLoggerMock = new Mock<ILog>();
				PrivateInspector.SetPrivateVariable(Sut, "_innerLogger",_innerLoggerMock.Object);
			}

			public Type TheType { get; private set; }
			public Guid TheComponentId { get; private set; }
			public LogComponent TheLogComponent { get; private set; }

			public Logger Sut { get; private set; }

			public Action<object> GetObjectLogger(TestLogLevel logLevel)
			{
				switch(logLevel)
				{
					case TestLogLevel.Trace:
						return theObject=>Sut.Trace(theObject);
					case TestLogLevel.Debug:
						return theObject => Sut.Debug(theObject);
					case TestLogLevel.Info:
						return theObject => Sut.Info(theObject);
					case TestLogLevel.Warn:
						return theObject => Sut.Warn(theObject);
					case TestLogLevel.Error:
						return theObject => Sut.Error(theObject);
					case TestLogLevel.Fatal:
						return theObject => Sut.Fatal(theObject);
					default:
						throw new ArgumentOutOfRangeException("logLevel");
				}
			}

			public void VerifyObjectLoggerWasCalled(TestLogLevel logLevel, string theObject)
			{
				switch (logLevel)
				{
					case TestLogLevel.Trace:
						_innerLoggerMock.Verify(x=>x.Trace(It.IsAny<Action<FormatMessageHandler>>()),Times.Exactly(1));
						break;
					case TestLogLevel.Debug:
						_innerLoggerMock.Verify(x => x.Debug(It.IsAny<Action<FormatMessageHandler>>()), Times.Exactly(1));
						break;
					case TestLogLevel.Info:
						_innerLoggerMock.Verify(x => x.Info(It.IsAny<Action<FormatMessageHandler>>()), Times.Exactly(1));
						break;
					case TestLogLevel.Warn:
						_innerLoggerMock.Verify(x => x.Warn(It.IsAny<Action<FormatMessageHandler>>()), Times.Exactly(1));
						break;
					case TestLogLevel.Error:
						_innerLoggerMock.Verify(x => x.Error(It.IsAny<Action<FormatMessageHandler>>()), Times.Exactly(1));
						break;
					case TestLogLevel.Fatal:
						_innerLoggerMock.Verify(x => x.Fatal(It.IsAny<Action<FormatMessageHandler>>()), Times.Exactly(1));
						break;
					default:
						throw new ArgumentOutOfRangeException("logLevel");
				}
			}
		}

		public enum TestLogLevel
		{
			Trace,Debug,Info,Warn,Error,Fatal
		}

		[Test,Theory]
		public void CanLogObject(TestLogLevel logLevel)
		{
			var theObject = "testdata";

			var testContext = new TestContext();

			var logger = testContext.GetObjectLogger(logLevel);
			logger(theObject);

			testContext.VerifyObjectLoggerWasCalled(logLevel,theObject);
		}
	}
}
