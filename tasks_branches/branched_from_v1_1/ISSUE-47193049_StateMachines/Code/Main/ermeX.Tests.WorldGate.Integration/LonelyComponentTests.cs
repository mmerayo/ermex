using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Common.Logging.Simple;
using NUnit.Framework;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.SupportTypes.Handlers;
using ermeX.Tests.SupportTypes.Messages;
using ermeX.Tests.SupportTypes.Services;

namespace ermeX.Tests.Full.Integration
{
	[TestFixture]
	class LonelyComponentTests
	{
		[TestFixtureSetUp]
		public void OnStartUp()
		{
			if (LogManager.Adapter is NoOpLoggerFactoryAdapter)
				LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(LogLevel.All, true, true, true, "yyyy/MM/dd HH:mm:ss:fff");
		}

		[TearDown]
		public void OnTearDown()
		{
			WorldGate.Reset();
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanStartLonelyComponent(DbEngineType dbType)
		{
			var testContext = new TestContext(dbType);
			WorldGate.ConfigureAndStart(testContext.Configuration);
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanStartLonelyComponentSeveralTimes(DbEngineType dbType)
		{
			int times = 10;
			var testContext = new TestContext(dbType);
			while (times-- > 0)
			{
				try
				{
					WorldGate.ConfigureAndStart(testContext.Configuration);
					WorldGate.Reset();
				}
				catch (Exception ex)
				{
					throw new Exception("times=" + times, ex);
				}
			}
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanResetLonelyComponent(DbEngineType dbType)
		{
			var testContext = new TestContext(dbType);
			WorldGate.ConfigureAndStart(testContext.Configuration);
			WorldGate.Reset();
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanStartLonelyComponentWithServicesAndSubscriptions(DbEngineType dbType)
		{
			const int times = 2;
			var testContext = new TestContext(dbType);
			for (var i = 0; i < times; i++)
			{
				WorldGate.ConfigureAndStart(testContext.Configuration);

				var handler = WorldGate.Suscribe<AnotherMessageHandlerA>();
				WorldGate.RegisterService<IServiceA>(typeof(ServiceA));

				WorldGate.Reset();
			}
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanPublishMessagesToLonelyComponent(DbEngineType dbType)
		{
			const int times = 2;
			var testContext = new TestContext(dbType);
			for (var i = 0; i < times; i++)
			{
				WorldGate.ConfigureAndStart(testContext.Configuration);

				WorldGate.Publish(new MessageA());

				WorldGate.Reset();
			}
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanSubscribeToMessagesToLonelyComponent(DbEngineType dbType)
		{
			const int times = 2;
			var testContext = new TestContext(dbType);
			for (var i = 0; i < times; i++)
			{
				WorldGate.ConfigureAndStart(testContext.Configuration);

				var handler=WorldGate.Suscribe<AnotherMessageHandlerA>();

				WorldGate.Reset();
			}
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanPublishAndReceiveMessagesWhenLonelyComponent(DbEngineType dbType)
		{
			const int times = 2;
			var testContext = new TestContext(dbType);
			for (var i = 0; i < times; i++)
			{
				WorldGate.ConfigureAndStart(testContext.Configuration);

				var handler = WorldGate.Suscribe<AnotherMessageHandlerA>();

				var expected = new MessageA();
				WorldGate.Publish(expected);

				Thread.Sleep(3000); //TODO: PULSE INSTEAD OF THIS bootch

				Assert.IsNotNull(handler.Message);
				Assert.AreEqual(expected.TheValue,handler.Message.TheValue);

				WorldGate.Reset();
			}
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanRegisterServicesToLonelyComponent(DbEngineType dbType)
		{
			const int times = 2;
			var testContext = new TestContext(dbType);
			for (var i = 0; i < times; i++)
			{
				WorldGate.ConfigureAndStart(testContext.Configuration);

				WorldGate.RegisterService<IServiceA>(typeof (ServiceA));


				WorldGate.Reset();
			}
		}

		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanRequestServicesPublishedByLonelyComponent(DbEngineType dbType)
		{
			const int times = 2;
			var testContext = new TestContext(dbType);
			for (var i = 0; i < times; i++)
			{
				WorldGate.ConfigureAndStart(testContext.Configuration);

				WorldGate.RegisterService<IServiceA>(typeof(ServiceA));

				var serviceProxy = WorldGate.GetServiceProxy<IServiceA>();

				Assert.IsNotNull(serviceProxy);

				var actual=serviceProxy.MethodReturnsTodayTicks();

				Assert.AreEqual(DateTime.Today.Ticks,actual);

				WorldGate.Reset();
			}
		}

		private class TestContext
		{
			private readonly Guid _componentId = Guid.NewGuid();

			public TestContext(DbEngineType dbType)
			{
				Configuration = Configurer.Configure(ComponentId)
				                          .ListeningToTcpPort(new TestPort(23332));
				switch (dbType)
				{
					case DbEngineType.Sqlite:
						Configuration=Configuration.SetSqliteDb(TestSettingsProvider.GetConnString(dbType));//TODO: THE DB MUST BE PERSISTENT AND BASED ON THE COMPONENT GUID AND ONLY ONE OPTION TO MOVE IT TO MEMORY
						break;
					case DbEngineType.SqliteInMemory:
						Configuration = Configuration.SetInMemoryDb();
						break;
					default:
						throw new ArgumentOutOfRangeException("dbType");
				}
				
			}

			public Configurer Configuration { get; private set; }

			public Guid ComponentId
			{
				get { return _componentId; }
			}

			
		}
	}
}
