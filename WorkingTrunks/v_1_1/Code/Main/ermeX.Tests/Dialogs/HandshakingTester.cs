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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ermeX;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.Common;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Entities.Entities;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Builtin.SuperSockets;
using ermeX.Tests.Services.Mock;

using ermeX.Tests.WorldGateTests.Mock;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Tests.Dialogs
{
    //[TestFixture]
    internal sealed class HandshakingTester : DialogTesterBase
    {
      

        private static TimeSpan _timeSpanWait = new TimeSpan(0, 0, 10);

       


        #region StartUpMessages

        [Ignore("TODO")]
        [Test]
        public void Component_Wont_Handle_Events_Raised_Before_StartUp_Is_Finished()
        {
            //como cuando se conecta solicita explicitamente toda la posible informacion de los eventos(ComponentEvent) que puede recibir, no procesa los creados antes del start up ya que no es necesario 
            //o sea, que no admite peticiones hasta que esta started
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void Sends_JoinRequestMessage_OnStartUp(DbEngineType dbEngine)

        {
            List<DummySocketServerResult> dummyResult;
            int remotePort;
            using (var localPort = new TestPort(2000))
            {
                AutoResetEvent eventDone;

                using (
                    var server1 = CreateThirdServerComponentForRequestJoinTo(dbEngine, RemoteComponentId,
                                                                             out dummyResult, out remotePort,
                                                                             out eventDone))
                {
                    var serviceLayerSettingsSource = TestSettingsProvider.GetServiceLayerSettingsSource(
                        LocalComponentId, dbEngine)
                        .RequestJoinTo(Networking.GetLocalhostIp(), remotePort, RemoteComponentId).ListeningToTcpPort(localPort);
                    WorldGate.ConfigureAndStart(serviceLayerSettingsSource);

                    eventDone.WaitOne(_timeSpanWait);

                    AssertReceivedJoinRequestMessage(serviceLayerSettingsSource,
                                                     ServiceOperationAttribute.GetOperationIdentifier(
                                                         typeof (IHandshakeService), "RequestJoinNetwork"), server1);
                }
            }

        }


        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void WhenRequest_JoinRequestMessage_UpdatesRequesterComponents(DbEngineType dbEngine)
        {

            List<DummySocketServerResult> dummyResult;
            int remotePort;
            AutoResetEvent eventDone ;
            var localPort = new TestPort(2000);
            using (
                var server1 = CreateThirdServerComponentForRequestJoinTo(dbEngine,RemoteComponentId, out dummyResult, out remotePort,out eventDone))
            {
                var serviceLayerSettingsSource = TestSettingsProvider.GetServiceLayerSettingsSource(
                    LocalComponentId, dbEngine)
                    .RequestJoinTo(Networking.GetLocalhostIp(), remotePort, RemoteComponentId).ListeningToTcpPort(localPort);
                WorldGate.ConfigureAndStart(serviceLayerSettingsSource);

                eventDone.WaitOne(_timeSpanWait);

            }
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            var components = dataAccessTestHelper.GetObjectsFromDb<AppComponent>(AppComponent.TableName);
            Assert.IsTrue(components.Count == 2);
            Assert.IsTrue(components.Count(x => x.ComponentId == RemoteComponentId) == 1);
            Assert.IsTrue(components.Count(x => x.ComponentId == LocalComponentId) == 1);

            var connectivities =dataAccessTestHelper. GetObjectsFromDb<ConnectivityDetails>(ConnectivityDetails.TableName);
            Assert.IsTrue(connectivities.Count(x => x.ComponentOwner == LocalComponentId) == 2);
            Assert.IsTrue(connectivities.Count(x => x.ServerId == LocalComponentId) == 1);
            Assert.IsTrue(connectivities.Count(x => x.ServerId == RemoteComponentId) == 1);
        }


        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void WhenReceived_JoinRequestMessage_Sends_MyComponentsMessage_To_The_Caller(DbEngineType dbEngine)
        {
            //Arrange
            var localPort = new TestPort(2000);
            var serviceLayerSettingsSource = TestSettingsProvider.GetServiceLayerSettingsSource(
                RemoteComponentId, dbEngine).ListeningToTcpPort(localPort);
            WorldGate.ConfigureAndStart(serviceLayerSettingsSource);

            ServiceRequestMessage request;
            ConnectivityDetails connectivityDetails;
            CreateJoinRequestNetworkMessage(dbEngine, RemoteComponentId,out connectivityDetails, out request);

            //Act
            var result = DoSendMessage(new ServerInfo(connectivityDetails), request);

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Ok);
            Assert.IsNotNull(result.ResultData);

            var data = result.ResultData as MyComponentsResponseMessage;
            Assert.IsNotNull(data);
            Assert.AreEqual(LocalComponentId, data.SourceComponentId);
            Assert.IsTrue(data.DateCreated.Date == DateTime.Today);
            Assert.IsFalse(data.CorrelationId.IsEmpty());
            Assert.IsTrue(data.Components.Count == 2);
            Assert.IsTrue(data.Components.Count(x => x.Item1.ComponentId == RemoteComponentId) == 1);
            Assert.IsTrue(data.Components.Count(x => x.Item1.ComponentId == LocalComponentId) == 1);
        }


        //TODO: refactor dialogs
        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void WhenRequest_JoinRequestMessage_SendsJoinRequestMesage_To_ReceivedComponents(DbEngineType dbEngine)
        {
            //create new FriendComponent
            List<DummySocketServerResult> dummyResultFriend;
            int remotePortFriend;
            AutoResetEvent eventDoneFriend;
            var localPort = new TestPort(2000);
            using (var friendSrv = CreateThirdServerComponentForRequestJoinTo(dbEngine,RemoteComponentId, out dummyResultFriend, out remotePortFriend,
                                                                       out eventDoneFriend))
            {

                //Create new third component
                List<DummySocketServerResult> dummyResultThird;
                int remotePortThird;
                AutoResetEvent eventDoneThird;
                var thirdComponentId = Guid.NewGuid();

                //add third component to MyComponents response from friend
                Guid operationIdentifier = ServiceOperationAttribute.GetOperationIdentifier(typeof (IHandshakeService), "RequestJoinNetwork");
                var result = (MyComponentsResponseMessage)friendSrv.DummyResults.Single(x => x.OperationId == operationIdentifier).ResultForResponse;
                var tuple = new Tuple<AppComponent, ConnectivityDetails>(
                    new AppComponent()
                        {
                            ComponentOwner = RemoteComponentId,
                            ComponentId = thirdComponentId,
                            Version = DateTime.Now.Ticks,
                            Id = 10000
                        },
                    new ConnectivityDetails()
                        {
                            ComponentOwner = RemoteComponentId,
                            Ip = Networking.GetLocalhostIp(),
                            ServerId = thirdComponentId,
                        });
                result.Components.Add(tuple);

                Configurer serviceLayerSettingsSource;

                //db settings                

                using (
                    var thirdSrv = CreateThirdServerComponentForRequestJoinTo(dbEngine, thirdComponentId, out dummyResultThird, out remotePortThird,
                                                                              out eventDoneThird))
                {
                    result.Components[result.Components.Count - 1].Item2.Port = remotePortThird;


                    //Join network
                    var layerSettingsSource = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
                    serviceLayerSettingsSource = layerSettingsSource.RequestJoinTo(Networking.GetLocalhostIp(), remotePortFriend, RemoteComponentId).ListeningToTcpPort(localPort);
                    WorldGate.ConfigureAndStart(serviceLayerSettingsSource);

                    eventDoneThird.WaitOne(_timeSpanWait);
                    eventDoneFriend.WaitOne(_timeSpanWait);


                    //assert third component was requested
                    AssertReceivedJoinRequestMessage(serviceLayerSettingsSource, operationIdentifier, thirdSrv);
                }
                //assert Friend component was requested
                AssertReceivedJoinRequestMessage(serviceLayerSettingsSource, operationIdentifier, friendSrv);

                //assert db objects
                DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
                var components = dataAccessTestHelper.GetObjectsFromDb<AppComponent>(AppComponent.TableName);
                Assert.IsTrue(components.Count == 3);
                Assert.IsTrue(components.Count(x => x.ComponentId == RemoteComponentId) == 1);
                Assert.IsTrue(components.Count(x => x.ComponentId == LocalComponentId) == 1);
                Assert.IsTrue(components.Count(x => x.ComponentId == thirdComponentId) == 1);

                var connectivities = dataAccessTestHelper.GetObjectsFromDb<ConnectivityDetails>(ConnectivityDetails.TableName);
                Assert.IsTrue(connectivities.Count(x => x.ComponentOwner == LocalComponentId) == 3);
                Assert.IsTrue(connectivities.Count(x => x.ServerId == LocalComponentId) == 1);
                Assert.IsTrue(connectivities.Count(x => x.ServerId == RemoteComponentId) == 1);
                Assert.IsTrue(connectivities.Count(x => x.ServerId == thirdComponentId) == 1);
            }

           
        }

        //the initial component that was requested to be joined to is not requested twice
        [Ignore("TODO")]
        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void WhenRequest_JoinRequestMessage_Does_Not_SendJoinRequestMesage_Twice_To_SameComponent(DbEngineType dbEngine)
        {
            //similar than previous checking that the calls
            throw new NotImplementedException();

        }


        [Ignore("TODO")]
        [Test]
        public void Caller_Waits_MyComponents_Message_response_With_TimeOut()
        {
            throw new NotImplementedException("raise exception");
        }


        [Ignore("TODO")]
        [Test]
        public void When_Joined_Updates_Local_Message_Definitions()
        {
            //TODO:TRACK SESSION: AND TESTS
            throw new NotImplementedException();
        }

        [Ignore("TODO")]
        [Test]
        public void When_Joined_Updates_Local_NonSystem_Service_Definitions()
        {
            //TODO:TRACK SESSION: AND TESTS
            throw new NotImplementedException();
        }

        #endregion       
    }
}