using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using ermeX.Logging;

namespace ermeX.Tests.Logging
{
	[TestFixture]
	public class LoggerTests
	{
		private class TestContext
		{
			Fixture _fixture;

			public TestContext()
			{
				_fixture = new Fixture();
				TheType = GetType();
				TheComponentId = Guid.NewGuid();
				TheLogComponent= _fixture.Create<LogComponent>();

				mock inner logger

				Sut= new Logger(TheType, TheComponentId, TheLogComponent);
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
		}
	}
}
