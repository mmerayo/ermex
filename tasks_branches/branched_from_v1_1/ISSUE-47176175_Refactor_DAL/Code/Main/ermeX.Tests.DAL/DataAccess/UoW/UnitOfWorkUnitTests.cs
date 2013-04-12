//using System;
//using System.Reflection;
//using Moq;
//using NHibernate;
//using NUnit.Framework;
//using ermeX.DAL.DataAccess.UoW;
//using ermeX.Tests.Common.Reflection;

//namespace ermeX.Tests.DAL.DataAccess.UoW
//{
//    [TestFixture]
//    public class UnitOfWorkUnitTests
//    {
//        private MockedFactory _mockedFactory;

//        [SetUp]
//        public void SetupContext()
//        {
//            _mockedFactory = new MockedFactory();
//        }

//        [TearDown]
//        public void OnTearDown()
//        {
//            _mockedFactory.Dispose();
//        }

//        [Test]
//        public void CanStart_WhenStartedAlready()
//        {
//            _mockedFactory.WithCreate();
//            UnitOfWork.Start();
//            Assert.Throws<InvalidOperationException>(() => UnitOfWork.Start());
//            _mockedFactory.VerifyAll();
//        }

//        [Test]
//        public void CanAccessCurrent()
//        {
//            _mockedFactory.WithCreate();
//            IUnitOfWork uow = UnitOfWork.Start();
			
//            var current = UnitOfWork.Current;
			
//            Assert.AreSame(uow, current);
//            _mockedFactory.VerifyAll();
//        }

//        [Test]
//        public void NotStartedUnitOfWork_throws()
//        {
//            _mockedFactory.WithoutCreate();
//            Assert.Throws<InvalidOperationException>(() =>
//                {
//                    var a = UnitOfWork.Current;
//                });
//        }

//        [Test]
//        public void IsStarted_IsAssigned()
//        {
//            _mockedFactory.WithCreate();
//            Assert.IsFalse(UnitOfWork.IsStarted);

//            var uow = UnitOfWork.Start();

//            Assert.IsTrue(UnitOfWork.IsStarted);
//            _mockedFactory.VerifyAll();
//        }

//        [Test]
//        public void CanGetValidCurrentSession_WhenStarted()
//        {
//            _mockedFactory.WithCreate();
//            _mockedFactory.WithSession();
//            using (UnitOfWork.Start())
//            {
//                ISession session = UnitOfWork.CurrentSession;
//                Assert.IsNotNull(session);
//            }
//            _mockedFactory.VerifyAll();
//        }

//        private class MockedFactory : IDisposable //TODO: REFACTOR TO HAVE BASE CLASS?
//        {
//            private readonly Mock<IUnitOfWorkFactory> _mockFactory;
//            private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//            private readonly Mock<ISession> _mockSession;

//            public MockedFactory()
//            {
//                _mockSession = new Mock<ISession>();
//                _mockFactory = new Mock<IUnitOfWorkFactory>();
//                _mockUnitOfWork = new Mock<IUnitOfWork>();
//                PrivateInspector.SetStaticPrivateVariable(typeof(UnitOfWork),"_unitOfWorkFactory", _mockFactory.Object);

//            }

//            public void WithoutCreate()
//            {
//                _mockFactory.Setup(x => x.Create()).Throws<InvalidOperationException>();
//            }

//            public void WithSession()
//            {
//                _mockFactory.Setup(x=>x.CurrentSession).Returns(_mockSession.Object).Verifiable();
//            }
//            public void WithCreate()
//            {				
//                _mockFactory.Setup(x => x.Create()).Returns(_mockUnitOfWork.Object).Verifiable();
//            }

//            public void VerifyAll()
//            {
//                if (_mockFactory != null) _mockFactory.VerifyAll();
//                if (_mockUnitOfWork != null) _mockUnitOfWork.VerifyAll();
//            }

//            public void Dispose()
//            {
//                PrivateInspector.SetStaticPrivateProperty(typeof(UnitOfWork),"CurrentUnitOfWork", null);
//            }
//        }
//    }
//}