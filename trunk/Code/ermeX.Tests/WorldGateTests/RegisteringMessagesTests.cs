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
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Entities.Entities;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.SupportTypes.Handlers;
using ermeX.Tests.SupportTypes.Messages;


namespace ermeX.Tests.WorldGateTests
{
    [Category(TestCategories.CoreFunctionalTest)]
    //[TestFixture]
    internal class RegisteringMessagesTests : RegisteringTestsBase
    {

   

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_Message_Suscription_To_Object(
            DbEngineType dbEngine)
        {
            DoTestRegisterMessage(dbEngine, true);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_Message_Suscription_To_Type(DbEngineType dbEngine)
        {
            DoTestRegisterMessage(dbEngine, false);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_Message_Suscription_To_SeveralTypes(
            DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var testMessageHandler = WorldGate.Suscribe(typeof (ComposedMessageHandler));
            Assert.IsNotNull(testMessageHandler);
            AssertIsSubscribed(typeof (MessageA), dbEngine,1);
            AssertIsSubscribed(typeof (MessageB), dbEngine,1);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_Several_Message_Suscriptions_To_SameType(
            DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var testMessageHandler1 = WorldGate.Suscribe(typeof(ComposedMessageHandler));
            Assert.IsNotNull(testMessageHandler1);
            var testMessageHandler2 = WorldGate.Suscribe(typeof(MoreConcreteMessageHandlerA));
            Assert.IsNotNull(testMessageHandler2);
            AssertIsSubscribed(typeof(MessageA), dbEngine, 2);
            AssertIsSubscribed(typeof(MessageB), dbEngine, 1);
        }


        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Cant_Register_Message_Suscription_To_WrongType(
            DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            Assert.Throws<ArgumentException>(() => WorldGate.Suscribe<DummyDomainEntity>(typeof (string)));
        }

        private void DoTestRegisterMessage(DbEngineType dbEngine, bool suscribeInstance) //TODO: subscribe instance
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            //if (suscribeInstance)
            //{
            var testMessageHandler = WorldGate.Suscribe<TestMessageHandler>(typeof (TestMessageHandler));
            //}
            //else
            //{
            //    WorldGate.Suscribe<DummyDomainEntity>(typeof (TestMessageHandler));
            //}

            AssertIsSubscribed(typeof (DummyDomainEntity), dbEngine,1);
        }

        private void AssertIsSubscribed(Type messageType, DbEngineType dbEngine,int timesSubscribed)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            //assert it subscribed it as incomming
            List<IncomingMessageSuscription> incomingMessageSuscriptions =
                dataAccessTestHelper.GetList<IncomingMessageSuscription>().Where(
                    x => x.BizMessageFullTypeName == messageType.FullName).ToList();

            Assert.IsTrue(incomingMessageSuscriptions.Count==timesSubscribed);

            foreach (var incomingMessageSuscription in incomingMessageSuscriptions)
            {
                Assert.IsNotNull(incomingMessageSuscription);
                Assert.IsTrue(incomingMessageSuscription.ComponentOwner == LocalComponentId);
                Assert.IsTrue(incomingMessageSuscription.BizMessageFullTypeName == messageType.FullName);
            }
            //asseerts it subscribed it as outgoing
            List<OutgoingMessageSuscription> outgoingMessageSuscriptions = dataAccessTestHelper.GetList<OutgoingMessageSuscription>();
            var outgoingSubscription =
                outgoingMessageSuscriptions.SingleOrDefault(x => x.BizMessageFullTypeName == messageType.FullName);
            Assert.IsNotNull(outgoingSubscription);
            Assert.IsTrue(outgoingSubscription.ComponentOwner == LocalComponentId);
            Assert.IsTrue(outgoingSubscription.Component == LocalComponentId);
            Assert.IsTrue(outgoingSubscription.BizMessageFullTypeName == messageType.FullName);

        }
        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_RegisterSuscription(DbEngineType dbEngine)
        {
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
            WorldGate.ConfigureAndStart(cfg);

            var autoResetEvent = new AutoResetEvent(false);

            var handler = WorldGate.Suscribe<TestMessageHandler>(typeof(TestMessageHandler));
            handler.AutoResetEvent = autoResetEvent;

            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            List<IncomingMessageSuscription> incomingMessageSuscriptions =
                dataAccessTestHelper.GetObjectsFromDb<IncomingMessageSuscription>(IncomingMessageSuscription.TableName);

            Assert.IsNotNull(incomingMessageSuscriptions);
            var incomingMessageSuscription = incomingMessageSuscriptions.SingleOrDefault(
                x => x.BizMessageFullTypeName == typeof(DummyDomainEntity).FullName);
            Assert.IsNotNull(incomingMessageSuscription);
        }
    }
}