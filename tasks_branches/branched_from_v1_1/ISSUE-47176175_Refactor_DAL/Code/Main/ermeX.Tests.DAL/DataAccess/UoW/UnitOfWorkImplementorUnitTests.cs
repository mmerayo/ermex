using System;
using System.Data;
using System.Reflection;
using Moq;
using NHibernate;
using NUnit.Framework;
using ermeX.DAL.DataAccess.UoW;

namespace ermeX.Tests.DAL.DataAccess.UoW
{
	[TestFixture]
	public class UnitOfWorkImplementorUnitTests
	{
		private TestContext _testContext;

		[SetUp]
		public void SetupContext()
		{
			_testContext = new TestContext();
		}

		[Test]
		public void Can_Dispose_UnitOfWorkImplementor()
		{
			_testContext.WithDisposeExpected();
			var target = new UnitOfWorkImplementor(_testContext.Factory, _testContext.Session);
			target.Dispose();

			_testContext.VerifyAll();
		}

		[Test]
		public void Can_BeginTransaction()
		{
			_testContext.WithBeginTransaction();

			var target = new UnitOfWorkImplementor(_testContext.Factory, _testContext.Session);
			
			var transaction = target.BeginTransaction();
			Assert.IsNotNull(transaction);
			_testContext.VerifyAll();
		}

		[Test]
		public void CanBeginTransaction_WithIsolation()
		{
			const IsolationLevel isolationLevel = IsolationLevel.Serializable;
			_testContext.WithBeginTransaction(isolationLevel);


			var target = new UnitOfWorkImplementor(_testContext.Factory, _testContext.Session);
			var transaction = target.BeginTransaction(isolationLevel);
		
			Assert.IsNotNull(transaction);
			_testContext.VerifyAll();
		}

		[Test]
		public void Can_execute_TransactionalFlush()
		{
			_testContext.WithTransactionalFlush();

			var target = new UnitOfWorkImplementor(_testContext.Factory, _testContext.Session);

			target.TransactionalFlush();
			_testContext.VerifyAll();
		}

		[Test]
		public void Can_execute_TransactionalFlush_WithIsolationLevel()
		{
			const IsolationLevel isolationLevel = IsolationLevel.Serializable;
			_testContext.WithTransactionalFlush(isolationLevel);

			var target = new UnitOfWorkImplementor(_testContext.Factory, _testContext.Session);

			target.TransactionalFlush(isolationLevel);
			_testContext.VerifyAll();
		}

		private class TestContext
		{
			private readonly Mock<IUnitOfWorkFactory> _mockFactory;
			private readonly Mock<ISession> _mockSession;
			private readonly Mock<ITransaction> _mockTransaction = new Mock<ITransaction>();


			public TestContext()
			{
				_mockFactory = new Mock<IUnitOfWorkFactory>();
				_mockSession = new Mock<ISession>();
			}

			public IUnitOfWorkFactory Factory
			{
				get { return _mockFactory.Object; }
			}

			public ISession Session
			{
				get { return _mockSession.Object; }
			}

			public void WithDisposeExpected()
			{
				_mockFactory.Setup(x => x.DisposeUnitOfWork(It.IsAny<UnitOfWorkImplementor>())).Verifiable();
				_mockSession.Setup(x => x.Dispose()).Verifiable();
			}

			public void WithBeginTransaction(IsolationLevel ? level=null)
			{
				if(level.HasValue)
					_mockSession.Setup(x => x.BeginTransaction(level.Value)).Returns((ITransaction)null);
				else
					_mockSession.Setup(x => x.BeginTransaction()).Returns((ITransaction) null);
			}

			public void VerifyAll()
			{
				_mockFactory.VerifyAll();
				_mockSession.VerifyAll();
				_mockTransaction.VerifyAll();
			}

			public void WithTransactionalFlush(IsolationLevel level = IsolationLevel.ReadCommitted)
			{
				_mockSession.Setup(x => x.BeginTransaction(level)).Returns(_mockTransaction.Object);
				_mockTransaction.Setup(x=>x.Commit()).Verifiable();
				_mockTransaction.Setup(x => x.Dispose()).Verifiable();
			}


		}
	}
}
