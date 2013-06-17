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
using System.Threading;
using NUnit.Framework;
using ermeX;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.Common;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;

using ermeX.Models.Entities;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Services.Builtin.SuperSockets;
using ermeX.Tests.Services.Mock;
using ermeX.Tests.WorldGateTests.Mock;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Tests.Dialogs
{
    /// <summary>
    /// Dialog testing common
    /// </summary>
    internal abstract class DialogTesterBase :DataAccessTestBase
    {

        

        public override void OnTearDown()
        {
            WorldGate.Reset();
            TestService.Reset();

            base.OnTearDown();
        }

       

        protected ServiceResult DoSendMessage(ServerInfo serverInfo,ServiceRequestMessage toSend)
        {
            ServiceResult result = null;
            using (IMockTestClient client = new DummyTestSuperSocketClient(serverInfo))
            {
                byte[] arr = ObjectSerializer.SerializeObjectToByteArray(toSend);
                result = client.Execute(arr);

            }
            return result;
        }

        
        protected MyComponentsResponseMessage GetDummyMyComponentsMessageResult(Guid remoteComponentId)
        {
         
            //the local component from the servers perspective
            var tuple1 = new Tuple<AppComponent, ConnectivityDetails>(
                new AppComponent()
                    {
                        ComponentOwner = remoteComponentId,
                        ComponentId = remoteComponentId,
                        Version = DateTime.Now.Ticks,
                        Id = 9999
                    }, new ConnectivityDetails()
                           {
                               ComponentOwner = remoteComponentId,
                               Ip = Networking.GetLocalhostIp(),
                               ServerId = remoteComponentId
                           });

            //the remote component from the servers perspective
            var tuple2 = new Tuple<AppComponent, ConnectivityDetails>(
                new AppComponent()
                    {
                        ComponentOwner = remoteComponentId,
                        ComponentId = LocalComponentId,
                        Version =DateTime.Now.Ticks,
                        Id = 10000
                    },
                new ConnectivityDetails()
                    {
                        ComponentOwner = remoteComponentId,
                        Ip = Networking.GetLocalhostIp(),
                        ServerId = LocalComponentId
                    });
            return new MyComponentsResponseMessage(remoteComponentId,
                                                   new List<Tuple<AppComponent, ConnectivityDetails>>()
                                                       {
                                                           tuple1,
                                                           tuple2
                                                       });
        }

        protected MessageSuscriptionsResponseMessage GetDummyMessageSuscriptionsResponseMessageResult(Guid remoteComponentId)
        {
            //TODO: FILL THIS OBJECT WITH SUSCRIPTIONS
            //TODO: ALSO WHATS THE PURPOSE OF THE correlationId?
            Guid correlationId = Guid.NewGuid();
            return new MessageSuscriptionsResponseMessage(remoteComponentId, correlationId);
        }

        protected PublishedServicesResponseMessage GetDummyMessageServiceDefinitionsResponseResult(Guid remoteComponentId)
        {
            //TODO:ALSO WHATS THE PURPOSE OF THE correlationId?
            Guid correlationId = Guid.NewGuid();
            return new PublishedServicesResponseMessage(remoteComponentId,correlationId);
        }

        protected void CreateJoinRequestNetworkMessage(DbEngineType dbEngine, Guid remoteComponentId, out ConnectivityDetails connectivityDetails, out ServiceRequestMessage request)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            connectivityDetails = dataAccessTestHelper.GetObjectsFromDb<ConnectivityDetails>(ConnectivityDetails.TableName).
                Single(
                    x => x.ServerId == remoteComponentId);

            var localPort = new TestPort(9000);

            request = new ServiceRequestMessage
                          {
                              CallingContextId = Guid.NewGuid(),
                              Operation =
                                  ServiceOperationAttribute.GetOperationIdentifier(typeof (IHandshakeService),
                                                                                   "RequestJoinNetwork"),
                              ServerId = remoteComponentId,
                              Parameters = new Dictionary<string, ServiceRequestMessage.RequestParameter>(),
                          };

            var joinRequestMessage = new JoinRequestMessage(LocalComponentId, Networking.GetLocalhostIp(), localPort)
                                         {
                                             CorrelationId = Guid.NewGuid()
                                         };
            request.Parameters.Add("Param0",
                                   new ServiceRequestMessage.RequestParameter("Param0",
                                                                              joinRequestMessage));
        }

        protected DummyTestSuperSocketServer<ServiceRequestMessage> CreateThirdServerComponentForRequestJoinTo(DbEngineType dbEngine, Guid remoteComponentId, out List<DummySocketServerResult> dummyResult, out int remotePort
                                                                                                       , out AutoResetEvent eventDone)
        {
            
            eventDone = new AutoResetEvent(false);

            remotePort = new TestPort(9000);

            //the connectivity details
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            dataAccessTestHelper.InsertConnectivityDetailsRecord(remoteComponentId, LocalComponentId,
                                            Networking.GetLocalhostIp(),remotePort, false, remoteComponentId);

            //the operations
            Guid operationIdentifier;
            CreateServiceOperation(dbEngine, remoteComponentId, out operationIdentifier, typeof (IHandshakeService), "RequestJoinNetwork");
            var joinResult = new DummySocketServerResult(operationIdentifier,
                                                         GetDummyMyComponentsMessageResult(remoteComponentId));

            CreateServiceOperation(dbEngine, remoteComponentId, out operationIdentifier, typeof(IHandshakeService), "ExchangeComponentStatus");
			//TODO: COMMENTED IN BIG REFACTOR [https://www.pivotaltracker.com/story/show/47193049]
			//const string resultForResponse = ComponentStatus.Starting;
			//var componentStatusResult = new DummySocketServerResult(operationIdentifier, resultForResponse);

            CreateServiceOperation(dbEngine, remoteComponentId, out operationIdentifier, typeof(IMessageSuscriptionsService), "RequestSuscriptions");
            var messageSuscriptionsResult =
                new DummySocketServerResult(
                    ServiceOperationAttribute.GetOperationIdentifier(typeof (IMessageSuscriptionsService),
                                                                     "RequestSuscriptions"),
                    GetDummyMessageSuscriptionsResponseMessageResult(remoteComponentId));


            CreateServiceOperation(dbEngine, remoteComponentId, out operationIdentifier, typeof(IPublishedServicesDefinitionsService), "RequestDefinitions");
            var messagePublishedServicesResult =
                new DummySocketServerResult(
                    ServiceOperationAttribute.GetOperationIdentifier(typeof (IPublishedServicesDefinitionsService),
                                                                     "RequestDefinitions"),
                    GetDummyMessageServiceDefinitionsResponseResult(remoteComponentId));

            CreateServiceOperation(dbEngine, remoteComponentId, out operationIdentifier, typeof(IMessageSuscriptionsService), "AddSuscriptions");
            var messageAddSuscriptionsResult =
               new DummySocketServerResult(
                   ServiceOperationAttribute.GetOperationIdentifier(typeof(IMessageSuscriptionsService),
                                                                    "AddSuscriptions"),null);

            CreateServiceOperation(dbEngine, remoteComponentId, out operationIdentifier, typeof(IPublishedServicesDefinitionsService), "AddServices");
            var messageAddServicesResult =
              new DummySocketServerResult(
                  ServiceOperationAttribute.GetOperationIdentifier(typeof(IPublishedServicesDefinitionsService),
                                                                   "AddServices"), null);

            CreateServiceOperation(dbEngine, remoteComponentId, out operationIdentifier, typeof(IPublishedServicesDefinitionsService), "AddService");
            var messageAddServiceResult =
              new DummySocketServerResult(
                  ServiceOperationAttribute.GetOperationIdentifier(typeof(IPublishedServicesDefinitionsService),
                                                                   "AddService"), null);

            dummyResult = new List<DummySocketServerResult>()
                              {
                                  //componentStatusResult,
                                  joinResult
                                  ,messageSuscriptionsResult
                                  ,messagePublishedServicesResult
                                  ,messageAddSuscriptionsResult
                                  ,messageAddServicesResult
                                  ,messageAddServiceResult
                              };
            return new DummyTestSuperSocketServer<ServiceRequestMessage>(remotePort, eventDone, dummyResult);
        }

        private void CreateServiceOperation(DbEngineType dbEngine, Guid remoteComponentId, out Guid operationIdentifier,
                                            Type interfaceType, string methodName)
        {
            operationIdentifier = ServiceOperationAttribute.GetOperationIdentifier(interfaceType,
                                                                                   methodName);
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            dataAccessTestHelper.InsertServiceDetails(LocalComponentId, remoteComponentId, methodName, null,
                                 interfaceType.FullName, operationIdentifier, DateTime.UtcNow, true);
        }

        protected void AssertReceivedJoinRequestMessage(Configurer serviceLayerSettingsSource, Guid operationIdentifier,
                                                      DummyTestSuperSocketServer<ServiceRequestMessage> server)
        {
            var actual = server.ReceivedMessages.Where(x => x.Operation == operationIdentifier).ToList();
            Assert.IsNotEmpty(actual);

            Assert.IsTrue(actual.Count == 1);

            var data = (JoinRequestMessage) actual[0].Parameters["Param0"].ParameterValue;
            Assert.AreEqual(serviceLayerSettingsSource.GetSettings<IComponentSettings>().ComponentId, data.SourceComponentId);
            Assert.IsTrue(data.DateCreated.Date == DateTime.Today);
            Assert.IsFalse(data.CorrelationId.IsEmpty());
        }
    }
}