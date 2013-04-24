using System;
using Moq;
using NHibernate;
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Providers;
using ermeX.DAL.Transactions;
using ermeX.DAL.UnitOfWork;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.DataAccess.UoW
{
	[TestFixture]
	class UnitOfWorkFactoryUnitTests
	{
		private TestContext _testContext;

		[SetUp]
		public void SetupContext()
		{
			_testContext=new TestContext();
			_testContext.Setup();
		}


		[Test]
		public void CanCreate_UnitOfWork()
		{
			//_testContext.WithSession();
			_testContext.WithTransactionStarted(true);
			var factory = _testContext.Factory;
			var implementor = factory.Create(false);

			Assert.IsNotNull(implementor);
			Assert.IsNotNull(implementor.Session);
			Assert.AreEqual(FlushMode.Commit, implementor.Session.FlushMode);
			_testContext.VerifySessionWasCreated();
		}
		
		private class TestContext
		{
			Mock<ISession> _mockSession = null;
			Mock<ITransaction> _mockTransaction= null;
			private Mock<ISessionProvider> _sessionProvider;
			public IUnitOfWorkFactory Factory { get; private set; }

			public void Setup()
			{
				_sessionProvider=new Mock<ISessionProvider>();
				var genericTransactionProvider = new GenericTransactionProvider();
				Factory = new UnitOfWorkFactory(_sessionProvider.Object,
				                                null,
				                                genericTransactionProvider, genericTransactionProvider);
			}

			public void WithSession()
			{
				_mockSession = new Mock<ISession>();
				_mockSession.SetupProperty(x => x.FlushMode, FlushMode.Commit);
				_mockSession.Setup(x => x.IsOpen).Returns(true);
				_sessionProvider.Setup(x => x.OpenSession(false)).Returns(_mockSession.Object).Verifiable();
			}

			public void WithTransactionStarted(bool started)
			{
				WithSession();
				_mockTransaction=new Mock<ITransaction>();
				_mockTransaction.SetupGet(x => x.IsActive).Returns(started);
				_mockSession.SetupGet(x => x.Transaction).Returns(_mockTransaction.Object);
			}

			public void VerifySessionWasCreated()
			{
				_sessionProvider.VerifyAll();
			}

			
		}
	}
}
