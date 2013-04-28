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
using System.Text;
using System.Threading;
using Moq;
using NUnit.Framework;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Parallel.Queues;

namespace ermeX.Tests.Bus.Listening.Handlers.InternalMessageHandling
{
	internal class ReceptionMessageHandlerTester : DataAccessTestBase
	{
		private readonly List<ReceptionMessageDistributor.MessageDistributorMessage> _sentMessages =
			new List<ReceptionMessageDistributor.MessageDistributorMessage>();

		private readonly ManualResetEvent _messageReceived = new ManualResetEvent(false);

		private ReceptionMessageHandler GetInstance(IUnitOfWorkFactory factory,
		                                            Action<ReceptionMessageDistributor.MessageDistributorMessage>
		                                            	messageReceived, out IReceptionMessageDistributor mockedDistributor)
		{
			var settings = TestSettingsProvider.GetClientConfigurationSettingsSource();


			var mock = new Mock<IReceptionMessageDistributor>();
			mock.Setup(x => x.EnqueueItem(It.IsAny<ReceptionMessageDistributor.MessageDistributorMessage>())).Callback(
				messageReceived);

			mockedDistributor = mock.Object;

			var mock2 = new Mock<IQueueDispatcherManager>();

			var queueMock = mock2.Object;

			var receptionMessageHandler = new ReceptionMessageHandler(GetIncommingQueueReader(factory),
			                                                          GetIncommingQueueWritter(factory), mockedDistributor,
			                                                          settings, queueMock);
			return receptionMessageHandler;
		}

		public override void OnStartUp()
		{
			base.OnStartUp();
			_sentMessages.Clear();
			_messageReceived.Reset();

		}

		private void DealWithMessage(ReceptionMessageDistributor.MessageDistributorMessage message)
		{
			_sentMessages.Add(message);
			_messageReceived.Set();
		}

		private TransportMessage GetTransportMessage<TData>(TData data)
		{
			var bizMessage = new BizMessage(data);
			var busMessage = new BusMessage(LocalComponentId, bizMessage);
			var transportMessage = new TransportMessage(RemoteComponentId, busMessage);
			return transportMessage;
		}

		private class Dummy
		{
			public string TheValue { get; set; }
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void Can_Handle_Message(DbEngineType dbEngineType)
		{
			var factory = GetUnitOfWorkFactory(dbEngineType);
			var expected = GetTransportMessage(new Dummy() {TheValue = "Some Data"});

			IReceptionMessageDistributor distributor;
			using (var target = GetInstance(factory, DealWithMessage, out distributor))
			{
				target.Handle(expected);
				_messageReceived.WaitOne(TimeSpan.FromSeconds(20));
			}


			Assert.AreEqual(1, _sentMessages.Count); //assert it was pushed to the next stage
			var incomingMessagesDataSource = GetRepository<Repository<IncomingMessage>>(factory);
			var incomingMessages = incomingMessagesDataSource.FetchAll();
			Assert.AreEqual(1, incomingMessages.Count());
			var incomingMessage = incomingMessages.First();
			Assert.AreEqual(incomingMessage, _sentMessages[0].IncomingMessage);
			Assert.AreEqual(Message.MessageStatus.ReceiverReceived, incomingMessage.Status);


		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void Enqueues_NonDistributedMessages_OnStartUp(DbEngineType dbEngineType)
		{
			var factory = GetUnitOfWorkFactory(dbEngineType);
			var dataSource = GetRepository<Repository<IncomingMessage>>(factory);
			TransportMessage transportMessage = GetTransportMessage(new Dummy() {TheValue = "Sample entity"});

			var incomingMessage = new IncomingMessage(transportMessage.Data)
			                      	{
			                      		ComponentOwner = RemoteComponentId,

			                      		PublishedTo = LocalComponentId,
			                      		TimeReceivedUtc = DateTime.UtcNow,
			                      		SuscriptionHandlerId = Guid.Empty,
			                      		//important as the p0ending messages have not subscriber yet
			                      		Status = Message.MessageStatus.ReceiverReceived,
			                      	};
			dataSource.Save(incomingMessage);

			IReceptionMessageDistributor distributor;
			using (var target = GetInstance(factory, DealWithMessage, out distributor))
				_messageReceived.WaitOne(TimeSpan.FromSeconds(20));

			Assert.AreEqual(1, _sentMessages.Count); //assert it was pushed to the next stage

			Assert.AreEqual(incomingMessage, _sentMessages[0].IncomingMessage);
		}
	}
}
