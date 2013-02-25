using System;
using System.Reflection;
using Moq;
using NHibernate;
using NUnit.Framework;
using ermeX.DAL.Interfaces.UnitOfWork;
using UoW = ermeX.DAL.DataAccess.UnitOfWork.UnitOfWork;
namespace ermeX.Tests.DAL.Integration.UnitOfWork
{
    [TestFixture]
    public class UnitOfWork_Fixture
    {

        [Test]
        public void Can_Start_UnitOfWork()
        {
            var factoryMock = new Mock<IUnitOfWorkFactory>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            // brute force attack to set my own factory via reflection
            var fieldInfo = typeof (UoW).GetField("_unitOfWorkFactory",
                                                  BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, factoryMock);

            factoryMock.Setup(x => x.Create()).Returns(unitOfWorkMock.Object);

            Assert.DoesNotThrow(()=>UoW.Start());

        }
    }

    [TestFixture]
    public class UnitOfWork_With_Factory_Fixture
    {
        private Mock<IUnitOfWorkFactory> _factoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ISession> _sessionMock;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            ResetUnitOfWork();
        }

        [SetUp]
        public void SetupContext()
        {
            _factoryMock = new Mock<IUnitOfWorkFactory>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _sessionMock = new Mock<ISession>();

            InstrumentUnitOfWork();
            _factoryMock.Setup(x => x.Create()).Returns(_unitOfWorkMock.Object).Verifiable();
            _factoryMock.Setup(x => x.CurrentSession).Returns(_sessionMock.Object).Verifiable();
            
        }

        [TearDown]
        public void TearDownContext()
        {
            _factoryMock.VerifyAll();
            _unitOfWorkMock.VerifyAll();
            _sessionMock.VerifyAll();

            ResetUnitOfWork();
        }

        private void InstrumentUnitOfWork()
        {
            // brute force attack to set my own factory via reflection
            var fieldInfo = typeof(UoW).GetField("_unitOfWorkFactory",
                                BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, _factoryMock);
        }

        private void ResetUnitOfWork()
        {
            // assert that the UnitOfWork is reset
            var propertyInfo = typeof(UoW).GetProperty("CurrentUnitOfWork",
                                BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic);
            propertyInfo.SetValue(null, null, null);
            //var fieldInfo = typeof(UnitOfWork).GetField("_innerUnitOfWork",
            //                    BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
            //fieldInfo.SetValue(null, null);
        }

        [Test]
        public void Can_Start_and_Dispose_UnitOfWork()
        {
            IUnitOfWork uow = UoW.Start();
            uow.Dispose();
        }

        [Test]
        public void Can_access_current_unit_of_work()
        {
            IUnitOfWork uow = UoW.Start();
            var current = UoW.Current;
            uow.Dispose();
        }

        [Test]
        public void Accessing_Current_UnitOfWork_if_not_started_throws()
        {
            try
            {
                var current = UoW.Current;
            }
            catch (InvalidOperationException ex)
            { }
        }

        [Test]
        public void Starting_UnitOfWork_if_already_started_throws()
        {
            UoW.Start();
            try
            {
                UoW.Start();
            }
            catch (InvalidOperationException ex)
            { }
        }

        [Test]
        public void Can_test_if_UnitOfWork_Is_Started()
        {
            Assert.IsFalse(UoW.IsStarted);

            IUnitOfWork uow = UoW.Start();
            Assert.IsTrue(UoW.IsStarted);
        }

        [Test]
        public void Can_get_valid_current_session_if_UoW_is_started()
        {
            using (UoW.Start())
            {
                ISession session = UoW.CurrentSession;
                Assert.IsNotNull(session);
            }
        }

        [Test]
        public void Get_current_session_if_UoW_is_not_started_throws()
        {
            try
            {
                ISession session = UoW.CurrentSession;
            }
            catch (InvalidOperationException ex)
            { }
        }
    }

}
