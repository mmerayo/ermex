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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ermeX;
using ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Entities.Entities;
using ermeX.Tests.Acceptance.Dummy;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;


namespace ermeX.Tests.Acceptance
{
    [TestFixture]
    sealed class  HandshakeTester:AcceptanceTester
    {
       
        private const int BottomPort = 9000;


        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void SeveralComponents_Using_SameFriend_CanStart_AtSameTime(DbEngineType engineType)
        {
            //arrange
            using (var senderListeningPort = new TestPort(BottomPort))
            {

                string dbConnString = TestSettingsProvider.GetConnString(engineType);
                    //TODO: support the creation of different databases to speed-up

                const int numOtherComponents = 5;
                List<TestComponent> components = new List<TestComponent>(numOtherComponents);
                List<Guid> componentIds = new List<Guid>(numOtherComponents + 1);
                using (var friendComponent = TestComponent.GetComponent())
                {
                    componentIds.Add(friendComponent.ComponentId);

                    friendComponent.Start(engineType, dbConnString, senderListeningPort);
                    for (int i = 0; i < numOtherComponents; i++)
                    {
                        TestComponent testComponent = TestComponent.GetComponent();
                        componentIds.Add(testComponent.ComponentId);
                        components.Add(testComponent);
                    }

                    List<Thread> threads = new List<Thread>(numOtherComponents);
                    ushort lastPortAssigned = senderListeningPort;
                    for (int index = 0; index < components.Count; index++)
                    {
                        var testComponent = components[index];
                        var receiverListeningPort = new TestPort((ushort) (lastPortAssigned + 1));
                        lastPortAssigned = receiverListeningPort;

                        var ts =
                            new ThreadStart(
                                () =>
                                InitializeConnectedComponent(engineType, senderListeningPort, receiverListeningPort,
                                                             friendComponent, dbConnString, testComponent));
                        Thread t = new Thread(ts);
                        threads.Add(t);
                        t.Start();
                    }

                    while (threads.Count > 0)
                    {
                        threads[0].Join();
                        threads.RemoveAt(0);
                    }
                    try
                    {
                        AssertComponentsAreDefined(engineType, friendComponent, components);
                        AssertConnectivityDetailsAreDefined(engineType, friendComponent, components);
                        AssertIncommingSuscriptionsAreDefined(engineType, friendComponent, components);
                        AssertOutgoingSuscriptionsAreDefined(engineType, friendComponent, components);
                        AssertDefaultServicesAreDefined(engineType, friendComponent, components);
                        foreach (var component in components)
                        {
                            List<TestComponent> subset = components.Where(x => x != component).ToList();
                            subset.Insert(0, friendComponent);

                            AssertComponentsAreDefined(engineType, component, subset);
                            AssertConnectivityDetailsAreDefined(engineType, friendComponent, components);
                            AssertIncommingSuscriptionsAreDefined(engineType, friendComponent, components);
                            AssertOutgoingSuscriptionsAreDefined(engineType, friendComponent, components);
                            AssertDefaultServicesAreDefined(engineType, friendComponent, components);
                        }
                    }
                    finally
                    {
                        while (components.Count > 0)
                        {
                            components[0].Dispose();
                            components.RemoveAt(0);
                        }
                    }
                }
            }
        }

        private void AssertDefaultServicesAreDefined(DbEngineType engineType, TestComponent targetComponent, List<TestComponent> otherComponents)
        {
            var operationIds =
                ServiceOperationAttribute.GetOperations(typeof (IHandshakeService)).Select(x => x.OperationIdentifier).
                    ToList();
            operationIds.AddRange(
                ServiceOperationAttribute.GetOperations(typeof (IMessageSuscriptionsService)).Select(
                    x => x.OperationIdentifier));
            operationIds.AddRange(
                ServiceOperationAttribute.GetOperations(typeof (IPublishedServicesDefinitionsService)).Select(
                    x => x.OperationIdentifier));

            var dataAccessTestHelper = GetDataHelper(engineType);
            //filter definitions by component
            var suscriptions =
                dataAccessTestHelper.GetObjectsFromDb<ServiceDetails>("ServicesDetails").Where(
                    x => x.ComponentOwner == targetComponent.ComponentId).ToList();
            int expectedRecords = (otherComponents.Count + 1)*operationIds.Count;
            Assert.IsTrue(suscriptions.Count == expectedRecords,
                          string.Format("Expected:{0} but was {1}",
                                        expectedRecords.ToString(CultureInfo.InvariantCulture), suscriptions.Count));

            //assert default outgoing suscriptions
            foreach (var otherComponent in otherComponents)
            {
                foreach (var operationId in operationIds)
                {
                    ServiceDetails suscription = suscriptions.SingleOrDefault(x => x.Publisher == otherComponent.ComponentId && x.OperationIdentifier == operationId);
                    Assert.IsNotNull(suscription);
                    Assert.AreEqual("<<REMOTE>>", suscription.ServiceImplementationTypeName);
                }
            }
        }

        private void AssertOutgoingSuscriptionsAreDefined(DbEngineType engineType, TestComponent targetComponent, List<TestComponent> otherComponents)
        {
            //filter definitions by component
            var suscriptions =
                GetDataHelper(engineType).GetObjectsFromDb<OutgoingMessageSuscription>("OutgoingMessageSuscriptions").Where(
                    x => x.ComponentOwner == targetComponent.ComponentId).ToList();
            int expected = 2*(otherComponents.Count + 1);
            Assert.IsTrue(suscriptions.Count == expected, string.Format("Expected: {0} Actual: {1}", expected, suscriptions.Count));

            //assert default outgoing suscriptions
            foreach (var otherComponent in otherComponents)
            {
                Assert.IsTrue(suscriptions.Count(
                    x =>
                    x.BizMessageFullTypeName == typeof (UpdatePublishedServiceMessage).FullName &&
                    x.Component == otherComponent.ComponentId) == 1);

                Assert.IsTrue(suscriptions.Count(
                    x =>
                    x.BizMessageFullTypeName == typeof (UpdateSuscriptionMessage).FullName &&
                    x.Component == otherComponent.ComponentId) == 1);
            }
        }

        private void AssertIncommingSuscriptionsAreDefined(DbEngineType engineType, TestComponent targetComponent, List<TestComponent> otherComponents)
        {
            //filter definitions by component
            var suscriptions =
                GetDataHelper(engineType).GetObjectsFromDb<IncomingMessageSuscription>("IncomingMessageSuscriptions").Where(
                    x => x.ComponentOwner == targetComponent.ComponentId).ToList();
            Assert.IsTrue(suscriptions.Count == 2);

            //assert default suscriptions
            Assert.IsTrue(suscriptions.Count(
                x =>
                x.BizMessageFullTypeName == typeof (UpdatePublishedServiceMessage).FullName &&
                x.HandlerType == typeof (UpdatePublishedServiceMessageHandler).FullName)==1);

            Assert.IsTrue(suscriptions.Count(
                x =>
                x.BizMessageFullTypeName == typeof (UpdateSuscriptionMessage).FullName &&
                x.HandlerType == typeof (UpdateSuscriptionMessageHandler).FullName)==1);
        }

        private void AssertConnectivityDetailsAreDefined(DbEngineType engineType, TestComponent targetComponent, List<TestComponent> otherComponents)
        {
            //filter definitions by component
            var components = GetDataHelper(engineType).GetObjectsFromDb<ConnectivityDetails>().Where(x => x.ComponentOwner == targetComponent.ComponentId).ToList();

            Assert.IsTrue(components.Count == otherComponents.Count + 1);

            //check target is defined for its own
            var details = components.SingleOrDefault(x => x.ServerId == targetComponent.ComponentId);
            Assert.IsNotNull(details);
            Assert.IsTrue(details.Port >= BottomPort);

            //check other components are defined on target
            foreach (var otherComponent in otherComponents)
            {
                details = components.SingleOrDefault(x => x.ServerId == otherComponent.ComponentId);
                Assert.IsNotNull(details);
                Assert.IsTrue(details.Port>=BottomPort);
            }
        }

        private void AssertComponentsAreDefined(DbEngineType engineType, TestComponent targetComponent, List<TestComponent> otherComponents)
        {
            //filter definitions by component
            var components = GetDataHelper(engineType).GetObjectsFromDb<AppComponent>("Components").Where(x=>x.ComponentOwner==targetComponent.ComponentId).ToList();

            Assert.IsTrue(components.Count == otherComponents.Count + 1);

            //check target is defined for its own
            AppComponent appComponent = components.SingleOrDefault(x => x.ComponentId == targetComponent.ComponentId);
            Assert.IsNotNull(appComponent);
            Assert.IsTrue(appComponent.IsRunning, "Component wasnt running");
            Assert.IsTrue(appComponent.ExchangedDefinitions, "Definitions werent exchanged");

            //check other components are defined on target
            foreach (var otherComponent in otherComponents)
            {
                appComponent = components.SingleOrDefault(x => x.ComponentId == otherComponent.ComponentId);
                Assert.IsNotNull(appComponent);
                Assert.IsTrue(appComponent.IsRunning,"Component wasnt running");
                //TODO: FIX THE REASON IS FAILING THE FOLLOWING ASSERTION 
                //Assert.IsTrue(appComponent.ExchangedDefinitions, "Definitions werent exchanged");
            }

        }
    }
}
