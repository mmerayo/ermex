// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Entities.Entities;
using ermeX.Tests.Common;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.RandomValues;
using ermeX.Tests.Common.SettingsProviders;

using ermeX.Tests.WorldGateTests.Mock;
using ermeX.Tests.WorldGateTests.Mock.Messages;

namespace ermeX.Tests.WorldGateTests
{
    [Category(TestCategories.CoreFunctionalTest)]
    //[TestFixture]
    internal class UsingMessagesClientTests : DataAccessTestBase
    {
        #region Setup/Teardown
        public override void OnTearDown()
        {
            WorldGate.Reset();
            TestService.Reset();

            base.OnTearDown();
        }
        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }
        #endregion

       

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Receive_PublishedMessage( DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var autoResetEvent = new AutoResetEvent(false);

            var handler = WorldGate.Suscribe<TestMessageHandlerOneMessage>(typeof(TestMessageHandlerOneMessage));
            handler.ExpectedMessages = 1;
            handler.SetReceivedEvent(autoResetEvent);
            var dummyDomainEntity = new DummyDomainEntity {Id = Guid.NewGuid()};
            WorldGate.Publish(dummyDomainEntity);

            autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
            var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity>();

            Assert.IsNotNull(lastEntityReceived);
            Assert.IsTrue(lastEntityReceived.Id == dummyDomainEntity.Id);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Receive_Several_Messages(DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var autoResetEvent = new AutoResetEvent(false);


            var handler = WorldGate.Suscribe<TestMessageHandlerSeveralMessages>();
            TestMessageHandlerSeveralMessages.AutoResetEvent = autoResetEvent;
            handler.ExpectedMessages = 1;
            var dummyDomainEntity = new DummyDomainEntity { Id = Guid.NewGuid() };
            WorldGate.Publish(dummyDomainEntity);

            autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
            var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity>();

            Assert.IsNotNull(lastEntityReceived);
            Assert.IsTrue(lastEntityReceived.Id == dummyDomainEntity.Id);

            handler.Clear();
            handler.ExpectedMessages = 1;
            autoResetEvent.Reset();
            var dummyDomainEntity2 = new DummyDomainEntity2 { Data = RandomHelper.GetRandomString()};
            WorldGate.Publish(dummyDomainEntity2);

            autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
            var lastEntityReceived2 = handler.LastEntityReceived<DummyDomainEntity2>();

            Assert.IsNotNull(lastEntityReceived2);
            Assert.IsTrue(lastEntityReceived2.Data== dummyDomainEntity2.Data);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void BaseTypeHandler_Receives_Inherited_Message(DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                  SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var autoResetEvent = new AutoResetEvent(false);

            var handler = WorldGate.Suscribe<TestMessageHandlerBaseType>();
            TestMessageHandlerBaseType.AutoResetEvent = autoResetEvent;
            handler.ExpectedMessages= 1;
            var dummyDomainEntity = new DummyDomainEntity3 {Data=RandomHelper.GetRandomString(),DateTime=RandomHelper.GetRandomDateTime()};
            WorldGate.Publish(dummyDomainEntity);

            autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
            var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity3>();

            Assert.IsNotNull(lastEntityReceived);
            Assert.IsTrue(lastEntityReceived.Data == dummyDomainEntity.Data);
            Assert.IsTrue(lastEntityReceived.DateTime == dummyDomainEntity.DateTime);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void BaseTypeHandler_And_ConcreteHandlerType_Receives_Inherited_Message(DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                 SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var autoResetEvent = new AutoResetEvent(false);

            var handler = WorldGate.Suscribe<TestMessageHandlerSeveralMessagesWithInheritance>();
            handler.SetReceivedEvent( autoResetEvent);
            handler.ExpectedMessages = 2;
            var dummyDomainEntity = new DummyDomainEntity3 { Data = RandomHelper.GetRandomString(), DateTime = RandomHelper.GetRandomDateTime() };
            WorldGate.Publish(dummyDomainEntity);
          
            autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
            var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity3>();

            Assert.IsNotNull(lastEntityReceived);
            Assert.IsTrue(lastEntityReceived.Data == dummyDomainEntity.Data);
            Assert.IsTrue(lastEntityReceived.DateTime == dummyDomainEntity.DateTime);

            Assert.IsTrue(handler.ReceivedMessages.Count==2);
            var first = (DummyDomainEntity3) handler.ReceivedMessages[0];
            var second = (DummyDomainEntity3) handler.ReceivedMessages[1];
            Assert.IsTrue(first.Data == second.Data);
            Assert.IsTrue(first.DateTime == second.DateTime);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void InterfaceTypeHandler_And_ConcreteHandlerType_Receives_Inherited_Message(DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                 SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var autoResetEvent = new AutoResetEvent(false);

            var handler = WorldGate.Suscribe<TestMessageHandlerSeveralMessagesWithInterfacesInheritance>();
            handler.SetReceivedEvent( autoResetEvent);
            handler.ExpectedMessages = 2;
            var dummyDomainEntity = new DummyDomainEntity3 { Data = RandomHelper.GetRandomString(), DateTime = RandomHelper.GetRandomDateTime() };
            WorldGate.Publish(dummyDomainEntity);
          
            autoResetEvent.WaitOne(new TimeSpan(0, 0, 25));
            var lastEntityReceived = handler.LastEntityReceived<DummyDomainEntity3>();

            Assert.IsNotNull(lastEntityReceived);
            Assert.IsTrue(lastEntityReceived.Data == dummyDomainEntity.Data);
            Assert.IsTrue(lastEntityReceived.DateTime == dummyDomainEntity.DateTime);

            Assert.IsTrue(handler.ReceivedMessages.Count==2);
            var first = (DummyDomainEntity3) handler.ReceivedMessages[0];
            var second = (DummyDomainEntity3) handler.ReceivedMessages[1];
            Assert.IsTrue(first.Data == second.Data);
            Assert.IsTrue(first.DateTime == second.DateTime);
        }

        
       //todo: interfaces, abstract types etc
    }
}