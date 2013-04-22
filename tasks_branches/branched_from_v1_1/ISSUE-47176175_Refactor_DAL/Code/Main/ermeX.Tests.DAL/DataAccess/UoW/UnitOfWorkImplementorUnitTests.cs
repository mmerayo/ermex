using System;
using System.Data;
using System.Reflection;
using Moq;
using NHibernate;
using NUnit.Framework;
using ermeX.DAL.UnitOfWork;
using ermeX.Tests.Common.Reflection;

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
		public void BeginsTransactionProperly([Values(true,false) ]bool started)
		{
			_testContext.WithTransactionStarted(started);

			var target = new UnitOfWorkImplementor(_testContext.Factory, _testContext.Session);
			var transaction=(IGenericTransaction)PrivateInspector.GetPrivateVariableValue(target, "_transaction");
			if(!started)
				Assert.IsNotNull(transaction);
			else
				Assert.IsNull(transaction);
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

			
		
			public void WithTransactionStarted(bool started)
			{
				_mockTransaction.SetupGet(x => x.IsActive).Returns(started);
				_mockSession.SetupGet(x => x.Transaction).Returns(_mockTransaction.Object);

				if (!started)
					_mockSession.Setup(x => x.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(_mockTransaction.Object);
			}

			public void VerifyAll()
			{
				_mockFactory.VerifyAll();
				_mockSession.VerifyAll();
				_mockTransaction.VerifyAll();
			}

		}
	}
}
