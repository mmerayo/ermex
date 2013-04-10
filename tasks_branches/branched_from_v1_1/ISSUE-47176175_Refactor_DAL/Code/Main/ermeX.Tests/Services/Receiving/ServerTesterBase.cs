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
using System.IO;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Messages;
using ermeX.DAL.Interfaces.Services;
using ermeX.Domain.Implementations.Messages;
using ermeX.Domain.Implementations.Services;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.Helpers;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Mock;

using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Reception;

namespace ermeX.Tests.Services
{
	[Category(TestCategories.CoreUnitTest)]
	//[TestFixture]
	internal abstract class ServerTesterBase : DataAccessTestBase
	{
		#region Setup/Teardown

		public override void OnStartUp()
		{

			base.OnStartUp();
		}

		#endregion

		private readonly DummyClientConfigurationSettings _settings = new DummyClientConfigurationSettings();
		private readonly Guid ComponentId = Guid.NewGuid();

		private ServiceDetailsDataSource ServiceDetailsDs { get; set; }
		private IChunkedServiceRequestMessageDataSource ChunkedServiceRequestMessageDS { get; set; }

		private void DoCanReceiveMessageTest(bool chunked, int numOfMsgToSend = 1)
		{
			var serverInfo = new ServerInfo
				{
					Ip = Networking.GetLocalhostIp(AddressFamily.InterNetwork),
					IsLocal = false,
					Port = new TestPort(9000),
					ServerId = Guid.NewGuid()
				};

			using (
				IServer server = GetServerInstance(serverInfo))
			{
				var dummyServiceHandler = new DummyMessageHandler();

				server.RegisterHandler(DummyMessageHandler.OperationId, dummyServiceHandler);
				server.StartListening();

				var threads = new List<Thread>(numOfMsgToSend);
				for (int i = 0; i < numOfMsgToSend; i++)
				{
					var t = new Thread(() =>
						{
							var eventHandled = new AutoResetEvent(false);

							var expected = new DummyDomainEntity {Id = Guid.NewGuid()};

							dummyServiceHandler.AddEvent(expected.Id, eventHandled);
							var layerMessage =
								LayerMessagesHelper.GetLayerMessage<DummyDomainEntity, TransportMessage>(
									LayerMessagesHelper.LayerMessageType.Transport, expected);
							ServiceRequestMessage toSend =
								ServiceRequestMessage.GetForMessagePublishing(layerMessage);

							ServiceResult res = DoSendMessage(toSend, serverInfo, chunked);
							Assert.IsNotNull(res);
							Assert.IsTrue(res.Ok);
							eventHandled.WaitOne(new TimeSpan(0, 0, 10));
							Assert.IsNotNull(dummyServiceHandler.ReceivedMessages[expected.Id]);
						});

					threads.Add(t);
				}

				foreach (Thread thread in threads)
				{
					thread.Start();
				}
			}
		}

		private void DoCanReceiveCollectionMessageTest(bool chunked, int numOfMsgToSend = 1)
		{
			var serverInfo = new ServerInfo
				{
					Ip = Networking.GetLocalhostIp(AddressFamily.InterNetwork),
					IsLocal = false,
					Port = new TestPort(9000),
					ServerId = Guid.NewGuid()
				};

			using (
				IServer server = GetServerInstance(serverInfo))
			{
				var dummyServiceHandler = new DummyMessageHandler();

				server.RegisterHandler(DummyMessageHandler.OperationId, dummyServiceHandler);
				server.StartListening();


				var t = new Thread(() =>
					{
						var eventHandled = new AutoResetEvent(false);

						var expected = new DummyDomainEntity {Id = Guid.NewGuid()};
						expected.Dummies = new List<DummyDomainEntity>();
						for (int i = 0; i < numOfMsgToSend; i++)
							expected.Dummies.Add(new DummyDomainEntity {Id = Guid.NewGuid()});

						dummyServiceHandler.AddEvent(expected.Id, eventHandled);
						var layerMessage =
							LayerMessagesHelper.GetLayerMessage<DummyDomainEntity, TransportMessage>(
								LayerMessagesHelper.LayerMessageType.Transport, expected);
						ServiceRequestMessage toSend =
							ServiceRequestMessage.GetForMessagePublishing(layerMessage);

						ServiceResult res = DoSendMessage(toSend, serverInfo, chunked);
						Assert.IsNotNull(res);
						Assert.IsTrue(res.Ok);
						eventHandled.WaitOne(new TimeSpan(0, 0, 10));
						Assert.IsNotNull(dummyServiceHandler.ReceivedMessages[expected.Id]);
					});


				t.Start();
			}
		}


		protected List<ChunkedServiceRequestMessage> GetChunks(byte[] source, int chunksNumber)
		{
			var result = new List<ChunkedServiceRequestMessage>(chunksNumber);

			Guid correlationId = Guid.NewGuid();

			int len = source.Length/chunksNumber;

			for (int i = 0; i < chunksNumber; i++)
			{
				bool lastChunk = i == chunksNumber - 1;
				byte[] subArray = lastChunk ? source.SubArray(i*len) : source.SubArray(i*len, len);
				result.Add(new ChunkedServiceRequestMessage(correlationId, i, lastChunk,
				                                            ServerBase.ChunkedMessageOperation,
				                                            subArray));
			}
			return result;
		}

		protected ServiceResult DoSendMessage(ServiceRequestMessage toSend, ServerInfo serverInfo, bool chunked)
		{
			ServiceResult result = null;
			using (IMockTestClient client = GetTestClientInstance(serverInfo))
			{
				byte[] arr = ObjectSerializer.SerializeObjectToByteArray(toSend);
				if (!chunked)
					result = client.Execute(arr);
				else
				{
					List<ChunkedServiceRequestMessage> chunkedServiceRequestMessages = GetChunks(arr, 3);

					foreach (ChunkedServiceRequestMessage msg in chunkedServiceRequestMessages)
					{
						byte[] tmp = ObjectSerializer.SerializeObjectToByteArray(msg);
						result = client.Execute(tmp);
					}
				}
			}
			return result;
		}

		private void RefreshServiceDetailsDataSource(DbEngineType dbEngine)
		{
			DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
			IDalSettings dataAccessSettingsSource = dataAccessTestHelper.DataAccessSettings;
			ServiceDetailsDs = GetDataSource<ServiceDetailsDataSource>(dataAccessSettingsSource.ConfigurationSourceType);
			ChunkedServiceRequestMessageDS =
				GetDataSource<ChunkedServiceRequestMessageDataSource>(dataAccessSettingsSource.ConfigurationSourceType);
		}

		protected abstract IServer GetServerInstance(ServerInfo serverInfo);
		protected abstract IMockTestClient GetTestClientInstance(ServerInfo serverInfo);

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void CanReceiveChunkedMessage(DbEngineType dbEngine)
			//TODO: CHECK THIS DBENGINES
		{
			RefreshServiceDetailsDataSource(dbEngine);
			DoCanReceiveMessageTest(true);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void CanReceiveMessage(DbEngineType dbEngine)
			//TODO: CHECK THIS DBENGINES
		{
			RefreshServiceDetailsDataSource(dbEngine);
			DoCanReceiveMessageTest(false);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void CanReceiveMessageCollection(DbEngineType dbEngine)
		{
			const int messagesNum = 5000;
			RefreshServiceDetailsDataSource(dbEngine);

			DoCanReceiveCollectionMessageTest(false, messagesNum);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void CanReceiveSeveralMessages(DbEngineType dbEngine)
		{
			const int messagesNum = 600;
			RefreshServiceDetailsDataSource(dbEngine);

			DoCanReceiveMessageTest(false, messagesNum);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDbWithBool")]
		public void WhenService_DoesntExistsInDb_ReturnsError(DbEngineType dbEngine, bool localServer)
		{
			RefreshServiceDetailsDataSource(dbEngine);

			var serverInfo = new ServerInfo
				{
					Ip = Networking.GetLocalhostIp(AddressFamily.InterNetwork),
					IsLocal = localServer,
					Port = new TestPort(9000),
					ServerId = Guid.NewGuid()
				};
			Guid callingContextId = Guid.NewGuid();
			Guid operationIdentifier = Guid.NewGuid();
			ServiceRequestMessage toSend =
				ServiceRequestMessage.GetForServiceRequest<DummyDomainEntity>(serverInfo.ServerId, operationIdentifier,
				                                                              callingContextId);

			var eventHandled = new AutoResetEvent(false);
			//var dummyServiceHandler = new DummyServiceHandler(eventHandled);

			using (IServer server = GetServerInstance(serverInfo))
			{
				//server.RegisterHandler(DummyServiceHandler.OperationId, dummyServiceHandler);
				server.StartListening();
				ServiceResult res = DoSendMessage(toSend, serverInfo, false);
				Assert.IsNotNull(res);
				Assert.IsFalse(res.Ok);

				eventHandled.WaitOne(new TimeSpan(0, 0, 10));
			}
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDbWithBool")]
		public void WhenService_Is_RegisteredCallsHandler_Obj(
			DbEngineType dbEngine, bool localServer)
			//TODO: CHECK THIS DBENGINES
		{
			RefreshServiceDetailsDataSource(dbEngine);

			var expected = new Dictionary<string, object>();
			const string testParam = "testParamName";
			expected.Add(testParam, new DummyDomainEntity {Id = Guid.NewGuid()});
			;

			var serverInfo = new ServerInfo
				{
					Ip = Networking.GetLocalhostIp(AddressFamily.InterNetwork),
					IsLocal = localServer,
					Port = new TestPort(9000),
					ServerId = Guid.NewGuid()
				};
			Guid callingContextId = Guid.NewGuid();
			Guid operationIdentifier = Guid.NewGuid();
			ServiceRequestMessage toSend =
				ServiceRequestMessage.GetForServiceRequest<DummyDomainEntity>(serverInfo.ServerId, operationIdentifier,
				                                                              callingContextId, expected);

			var eventHandled = new AutoResetEvent(false);
			var dummyServiceHandler = new DummyServiceHandler(eventHandled);

			using (IServer server = GetServerInstance(serverInfo))
			{
				server.RegisterHandler(operationIdentifier, dummyServiceHandler);
				server.StartListening();
				ServiceResult res = DoSendMessage(toSend, serverInfo, false);
				Assert.IsNotNull(res);
				Assert.IsTrue(res.Ok);
				Assert.AreEqual(callingContextId, res.AsyncResponseId);
				eventHandled.WaitOne(new TimeSpan(0, 0, 10));
			}

			Assert.IsNotNull(dummyServiceHandler.ReceivedMessage);
			Assert.IsTrue(((DummyDomainEntity) expected[testParam]).Id ==
			              ((DummyDomainEntity) dummyServiceHandler.ReceivedMessage[testParam].ParameterValue).Id);
		}

		protected ICanWriteChunkedMessages GetChunkedMessagesWritter()
		{
			return new ChunkedMessagesWriter(ChunkedServiceRequestMessageDS);
		}

		protected ICanReadChunkedMessages GetChunkedMessagesReader()
		{
			return new ChunkedMessagesReader(ChunkedServiceRequestMessageDS);
		}

		protected ICanReadServiceDetails GetServiceDetailsReader()
		{
			return new ServiceDetailsReader(ServiceDetailsDs);
		}
	}
}