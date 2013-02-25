using System.Data;
using Moq;
using NHibernate;
using NUnit.Framework;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.UnitOfWork;

namespace ermeX.Tests.DAL.Integration.UnitOfWork
{
    [TestFixture]
    public class UnitOfWorkImplementor_Fixture
    {
        private Mock<IUnitOfWorkFactory> _factoryMock;
        private Mock<ISession> _sessionMock;
        private IUnitOfWorkImplementor _uowImpl;

        [SetUp]
        public void SetupContext()
        {
            _factoryMock = new Mock<IUnitOfWorkFactory>();
            _sessionMock = new Mock<ISession>();
        }

        [Test]
        public void Can_create_UnitOfWorkImplementor()
        {

            _uowImpl = new UnitOfWorkImplementor(_factoryMock.Object, _sessionMock.Object);
            Assert.AreSame(_factoryMock.Object, ((UnitOfWorkImplementor) _uowImpl).Factory);
            Assert.AreSame(_sessionMock.Object, ((UnitOfWorkImplementor) _uowImpl).Session);
        }

        [Test]
        public void Can_Dispose_UnitOfWorkImplementor()
        {

            _factoryMock.Setup(x => x.DisposeUnitOfWork(null)).Verifiable();
            _sessionMock.Setup(x => x.Dispose()).Verifiable();

            _uowImpl = new UnitOfWorkImplementor(_factoryMock.Object, _sessionMock.Object);
            _uowImpl.Dispose();

            _factoryMock.VerifyAll();
            _sessionMock.VerifyAll();
        }

        [Test]
        public void Can_Flush_UnitOfWorkImplementor()
        {
            _sessionMock.Setup(x => x.Flush()).Verifiable();

            _uowImpl = new UnitOfWorkImplementor(_factoryMock.Object, _sessionMock.Object);
            _uowImpl.Flush();
        }

        [Test]
        public void Can_BeginTransaction()
        {
            _sessionMock.Setup(x => x.BeginTransaction()).Returns((ITransaction)null);

            _uowImpl = new UnitOfWorkImplementor(_factoryMock.Object, _sessionMock.Object);
            var transaction = _uowImpl.BeginTransaction();
            Assert.IsNotNull(transaction);

            _sessionMock.VerifyAll();
        }

        [Test]
        public void Can_BeginTransaction_specifying_isolation_level()
        {
            var isolationLevel = IsolationLevel.Serializable;

            _sessionMock.Setup(x => x.BeginTransaction(isolationLevel)).Returns((ITransaction) null);

            _uowImpl = new UnitOfWorkImplementor(_factoryMock.Object, _sessionMock.Object);
            var transaction = _uowImpl.BeginTransaction(isolationLevel);
            Assert.IsNotNull(transaction);

            _sessionMock.VerifyAll();
        }

        [Test]
        public void Can_execute_TransactionalFlush()
        {
            var tx = new Mock<ITransaction>();
            var session = new Mock<ISession>();
            session.Setup(x => x.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(tx.Object);

            _uowImpl = new UnitOfWorkImplementor(_factoryMock.Object, _sessionMock.Object);

            tx.Setup(x => x.Commit()).Verifiable();
            tx.Setup(x => x.Dispose()).Verifiable();

            //_uowImpl = new UnitOfWorkImplementor(_factoryMock.Object, session.Object);
            _uowImpl.TransactionalFlush();

            tx.Verify(x=>x.Commit(),Times.Once());
            tx.Verify(x => x.Dispose(), Times.Once());
        }

        [Test]
        public void Can_execute_TransactionalFlush_specifying_isolation_level()
        {
            var tx = new Mock<ITransaction>();
            var session = new Mock<ISession>();
            session.Setup(x => x.BeginTransaction(IsolationLevel.Serializable)).Returns(tx.Object);

            _uowMock = _mocks.PartialMock<UnitOfWorkImplementor>(_factoryMock, session);

            using (_mocks.Record())
            {
                Expect.Call(tx.Commit);
                Expect.Call(tx.Dispose);
            }
            using (_mocks.Playback())
            {
                _uowMock.TransactionalFlush(IsolationLevel.Serializable);
            }
        }
    }
}