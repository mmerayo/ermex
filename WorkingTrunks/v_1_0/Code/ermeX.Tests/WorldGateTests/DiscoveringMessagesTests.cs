// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Common;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
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
    internal class DiscoveringMessagesTests : DataAccessTestBase
    {

      

        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }

        public override void OnTearDown()
        {
            WorldGate.Reset();
            base.OnTearDown();
        }

        #region testexecutors

        private List<IncomingMessageSuscription> DoCanSubscribeTest(DbEngineType dbEngine, Type[] excludeTypes,
                                                                    int expectedItems)
        {
            var cfg =TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                         new List<DataSchemaType>(){DataSchemaType.ClientComponent})
                .DiscoverSubscriptors(new[] {typeof (MessageA).Assembly},
                                      excludeTypes);

            WorldGate.ConfigureAndStart(cfg);
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);

            var actual = dataAccessTestHelper.GetObjectsFromDb<IncomingMessageSuscription>(IncomingMessageSuscription.TableName)
                .Where(x => !x.HandlerType.StartsWith("ermeX.Bus")).ToList(); //TODO: ADD FLAG the system

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Count == expectedItems);
            return actual;
        }

        #endregion

        #region Basic

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Subscribe_To_Messages_In_Assembly(DbEngineType dbEngine)
        {
            var actual = DoCanSubscribeTest(dbEngine, new[]
                                                          {
                                                              typeof (ComposedMessageHandler),
                                                              typeof (AnotherMessageHandlerA)
                                                          }, 2);
            Assert.IsTrue(actual.Count==2);

            int idxB;
            int idxA;
            if(actual[0].BizMessageFullTypeName==typeof(MessageB).FullName)
            {
                idxB = 0;
                idxA = 1;
            }
            else
            {
                idxA = 0;
                idxB = 1;
            }

            Assert.AreEqual(actual[idxB].BizMessageFullTypeName, typeof (MessageB).FullName);
            Assert.AreEqual(actual[idxB].HandlerType, typeof (MessageHandlerB).FullName);
            Assert.AreEqual(actual[idxA].BizMessageFullTypeName, typeof (MessageA).FullName);
            Assert.AreEqual(actual[idxA].HandlerType, typeof (MoreConcreteMessageHandlerA).FullName);

        }


        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Subscribe_To_Messages_In_Assembly_WithoutExcludingTypes(
            DbEngineType dbEngine)
        {
            var actual = DoCanSubscribeTest(dbEngine, null
                                            , 5);
        }


        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Subscribe_To_Messages_In_Assembly_When_HandlerHasSeveralInterfaces(
            DbEngineType dbEngine)
        {
            var actual = DoCanSubscribeTest(dbEngine, new[]
                                                          {
                                                              typeof (MoreConcreteMessageHandlerA),
                                                              typeof (MessageHandlerB),
                                                              typeof (AnotherMessageHandlerA)
                                                          }, 2);
            Assert.AreEqual(actual[0].BizMessageFullTypeName, typeof (MessageA).FullName);
            Assert.AreEqual(actual[0].HandlerType, typeof (ComposedMessageHandler).FullName);
            Assert.AreEqual(actual[1].BizMessageFullTypeName, typeof (MessageB).FullName);
            Assert.AreEqual(actual[1].HandlerType, typeof (ComposedMessageHandler).FullName);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Subscribe_To_Messages_In_Assembly_When_HandlerHasNonHandlerInterfaces(
            DbEngineType dbEngine)
        {
            var actual = DoCanSubscribeTest(dbEngine, new[]
                                                          {
                                                              typeof (MoreConcreteMessageHandlerA),
                                                              typeof (ComposedMessageHandler),
                                                              typeof (MessageHandlerB),
                                                              typeof (MessageHandlerA)
                                                          }, 1);
            var incomingMessageSuscription = actual[0];
            Assert.AreEqual(incomingMessageSuscription.BizMessageFullTypeName, typeof (MessageA).FullName);
            Assert.AreEqual(incomingMessageSuscription.HandlerType, typeof (AnotherMessageHandlerA).FullName);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Late_Subscribe_To_Messages_When_Handler_In_Other_Assemblies(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                         new List<DataSchemaType>(){DataSchemaType.ClientComponent})
                .DiscoverSubscriptors(new[] {typeof (MessageA).Assembly},
                                      new[]
                                          {
                                              typeof (ComposedMessageHandler),
                                              typeof (AnotherMessageHandlerA),
                                              typeof (MessageHandlerB)
                                          });

            WorldGate.ConfigureAndStart(cfg);
            WorldGate.Suscribe<TestMessageHandler>(typeof (TestMessageHandler));

            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            var actual = dataAccessTestHelper.GetObjectsFromDb<IncomingMessageSuscription>(IncomingMessageSuscription.TableName)
                .Where(x => !x.HandlerType.StartsWith("ermeX.Bus")).ToList();
                //TODO: ADD FLAG to show is a system subscription

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Count == 2);
            Assert.AreEqual(actual[0].BizMessageFullTypeName, typeof (MessageA).FullName);
        }
        #endregion 
    }
}