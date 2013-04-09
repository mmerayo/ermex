using System;
using Moq;
using NHibernate;
using NUnit.Framework;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.DataAccess.UoW;

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
			var implementor = factory.Create();

			Assert.IsNotNull(implementor);
			Assert.IsNotNull(factory.CurrentSession);
			Assert.AreEqual(FlushMode.Commit, factory.CurrentSession.FlushMode);
			_testContext.VerifySessionWasCreated();
		}

		[Test]
		public void AccessingCurrentSession_WhenNoSession_Throws()
		{
			Assert.Throws<InvalidOperationException>(() =>
				{
					var s = _testContext.Factory.CurrentSession;
				});
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
				Factory=new UnitOfWorkFactory(_sessionProvider.Object);
			}

			public void WithSession()
			{
				_mockSession = new Mock<ISession>();
				_mockSession.SetupProperty(x => x.FlushMode, FlushMode.Commit);
				_mockSession.Setup(x => x.IsOpen).Returns(true);
				_sessionProvider.Setup(x => x.OpenSession()).Returns(_mockSession.Object).Verifiable();
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
