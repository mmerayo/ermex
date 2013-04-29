// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ermeX;
using ermeX.Common;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Models.Entities;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.SupportTypes.Handlers;
using ermeX.Tests.SupportTypes.Messages;


namespace ermeX.Tests.WorldGateTests
{

    [Category(TestCategories.CoreSystemTest)]
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
            var cfg =TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine)
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

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
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


        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void Can_Subscribe_To_Messages_In_Assembly_WithoutExcludingTypes(
            DbEngineType dbEngine)
        {
            var actual = DoCanSubscribeTest(dbEngine, null
                                            , 5);
        }


        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
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

        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
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

        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void Can_Late_Subscribe_To_Messages_When_Handler_In_Other_Assemblies(DbEngineType dbEngine)
        {
            

            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine)
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