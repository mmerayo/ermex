using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;

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


		[Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionAllSqliteDbs)]
		public void CanStartLonelyComponent(DbEngineType dbType)
		{
			var testContext = new TestContext(dbType);
			WorldGate.ConfigureAndStart(testContext.Configuration);
		}

		[Test]
		public void CanResetLonelyComponent()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanStartLonelyComponentWithServicesAndSubscriptions()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanResetLonelyComponentWithServicesAndSubscriptions()
		{
			throw new NotImplementedException();
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
