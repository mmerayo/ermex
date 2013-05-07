﻿// /*---------------------------------------------------------------------------------------*/
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
using Moq;
using NUnit.Framework;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.LayerMessages;
using ermeX.Models.Entities;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Parallel.Queues;

namespace ermeX.Tests.Bus.Listening.Handlers.InternalMessageHandling.WorkflowHandlers
{
	internal class ReceptionMessageDistributorTester : DataAccessTestBase
	{
		private readonly List<QueueDispatcherManager.QueueDispatcherManagerMessage> _sentMessages =
			new List<QueueDispatcherManager.QueueDispatcherManagerMessage>();

		private readonly ManualResetEvent _messageReceived = new ManualResetEvent(false);

		private ReceptionMessageDistributor GetInstance(IUnitOfWorkFactory factory,
		                                                Action<QueueDispatcherManager.QueueDispatcherManagerMessage>
			                                                messageReceived, out IQueueDispatcherManager mockedDispatcher)
		{
			var mock = new Mock<IQueueDispatcherManager>();
			mock.Setup(x => x.EnqueueItem(It.IsAny<QueueDispatcherManager.QueueDispatcherManagerMessage>()))
			    .Callback(messageReceived);

			mockedDispatcher = mock.Object;

		    var result = new ReceptionMessageDistributor(GetIncommingMessageSubscriptionsReader(factory),
		                                                                      GetIncommingQueueReader(factory), GetIncommingQueueWritter(factory),
		                                                                      mockedDispatcher);
            result.Start();
		    return result;
		}

		public override void OnStartUp()
		{
			base.OnStartUp();
			_sentMessages.Clear();
			_messageReceived.Reset();

		}

		private void DealWithMessage(QueueDispatcherManager.QueueDispatcherManagerMessage message)
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

		private class Dummy2 : Dummy
		{
			public string TheValue2 { get; set; }
		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void Can_Distribute_Messages(DbEngineType dbEngineType)
		{
			var factory = GetUnitOfWorkFactory(dbEngineType);
			var dataSource = GetRepository<Repository<IncomingMessage>>(factory);
			TransportMessage transportMessage = GetTransportMessage(new Dummy() {TheValue = "Sample entity"});

			var incomingMessage = new IncomingMessage(transportMessage.Data)
				{
					ComponentOwner = RemoteComponentId,

					PublishedTo = LocalComponentId,
					TimeReceivedUtc = DateTime.UtcNow,
					SuscriptionHandlerId = Guid.Empty, //important as the p0ending messages have not subscriber yet
					Status = Message.MessageStatus.ReceiverReceived,
				};
			dataSource.Save(incomingMessage);

			IncomingMessageSuscription s1 = new IncomingMessageSuscription()
				{
					BizMessageFullTypeName = typeof (Dummy).FullName,
					ComponentOwner = LocalComponentId,
					SuscriptionHandlerId = Guid.NewGuid(),
					HandlerType = typeof (string).FullName
				};
			var incomingMessageSuscriptionsDataSource = GetRepository<Repository<IncomingMessageSuscription>>(factory);
			incomingMessageSuscriptionsDataSource.Save(s1);

			IQueueDispatcherManager mockedDispatcher;
			using (var target = GetInstance(factory, DealWithMessage, out mockedDispatcher))
			{
				target.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage));
				_messageReceived.WaitOne(TimeSpan.FromSeconds(10));
			}

			Assert.IsTrue(_sentMessages.Count == 1); // asserts the original was removed
			var queueDispatcherManagerMessage = _sentMessages[0];
			Assert.IsTrue(queueDispatcherManagerMessage.MustCalculateLatency); //It was enqueued to recalculate latency
			var incomingMessages = dataSource.FetchAll();
			Assert.IsTrue(incomingMessages.Count() == 1);
			var distributedMessage = incomingMessages.First();

			Assert.AreEqual(s1.SuscriptionHandlerId, distributedMessage.SuscriptionHandlerId);
			Assert.AreEqual(queueDispatcherManagerMessage.IncomingMessage, distributedMessage);


		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void Enqueues_NonDeliveredMessages_OnStartUp(DbEngineType dbEngineType)
		{
			var factory = GetUnitOfWorkFactory(dbEngineType);
			var dataSource = GetRepository<Repository<IncomingMessage>>(factory);
			var transportMessage = GetTransportMessage(new Dummy() {TheValue = "Sample entity"});

			var s1 = new IncomingMessageSuscription()
				{
					BizMessageFullTypeName = typeof (Dummy).FullName,
					ComponentOwner = LocalComponentId,
					SuscriptionHandlerId = Guid.NewGuid(),
					HandlerType = typeof (string).FullName
				};
			var incomingMessageSuscriptionsDataSource = GetRepository<Repository<IncomingMessageSuscription>>(factory); ;
			incomingMessageSuscriptionsDataSource.Save(s1);

			var incomingMessage = new IncomingMessage(transportMessage.Data)
				{
					ComponentOwner = RemoteComponentId,

					PublishedTo = LocalComponentId,
					TimeReceivedUtc = DateTime.UtcNow,
					SuscriptionHandlerId = s1.SuscriptionHandlerId,
					Status = Message.MessageStatus.ReceiverDispatchable,
				};
			dataSource.Save(incomingMessage);

			IQueueDispatcherManager mockedDispatcher;
			using (var target = GetInstance(factory, DealWithMessage, out mockedDispatcher))
				_messageReceived.WaitOne(TimeSpan.FromSeconds(10));

			Assert.IsTrue(_sentMessages.Count == 1); // asserts there is original was removed
			var queueDispatcherManagerMessage = _sentMessages[0];
			Assert.IsFalse(queueDispatcherManagerMessage.MustCalculateLatency); //It was enqueued to NOT TO recalculate latency
			var incomingMessages = dataSource.FetchAll();
			Assert.IsTrue(incomingMessages.Count() == 1);
			var distributedMessage = incomingMessages.First();

			Assert.AreEqual(s1.SuscriptionHandlerId, distributedMessage.SuscriptionHandlerId);
			Assert.AreEqual(queueDispatcherManagerMessage.IncomingMessage, distributedMessage);

		}

		[Ignore("Tehre are assertions in the other tests to probe this")]
		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void Removes_Source_Message_Once_Distributed(DbEngineType dbEngineType)
		{
			throw new NotImplementedException();
		}


		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void When_Subscribed_ToBase_Type_Will_DistributeIt_Correctly(DbEngineType dbEngineType)
		{
			var factory = GetUnitOfWorkFactory(dbEngineType);
			var dataSource = GetRepository<Repository<IncomingMessage>>(factory);
			TransportMessage transportMessage = GetTransportMessage(new Dummy2() {TheValue = "Sample entity", TheValue2 = "2222"});

			var incomingMessage = new IncomingMessage(transportMessage.Data)
				{
					ComponentOwner = RemoteComponentId,

					PublishedTo = LocalComponentId,
					TimeReceivedUtc = DateTime.UtcNow,
					SuscriptionHandlerId = Guid.Empty, //important as the p0ending messages have not subscriber yet
					Status = Message.MessageStatus.ReceiverReceived,
				};
			dataSource.Save(incomingMessage);

			IncomingMessageSuscription s1 = new IncomingMessageSuscription()
				{
					BizMessageFullTypeName = typeof (Dummy).FullName,
					ComponentOwner = LocalComponentId,
					SuscriptionHandlerId = Guid.NewGuid(),
					HandlerType = typeof (string).FullName
				};
			var incomingMessageSuscriptionsDataSource = GetRepository<Repository<IncomingMessageSuscription>>(factory);
			incomingMessageSuscriptionsDataSource.Save(s1);

			IQueueDispatcherManager mockedDispatcher;
			using (var target = GetInstance(factory, DealWithMessage, out mockedDispatcher))
			{
				target.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage));
				_messageReceived.WaitOne(TimeSpan.FromSeconds(10));
			}

			Assert.IsTrue(_sentMessages.Count == 1); // asserts the original was removed
			var queueDispatcherManagerMessage = _sentMessages[0];
			Assert.IsTrue(queueDispatcherManagerMessage.MustCalculateLatency); //It was enqueued to recalculate latency
			var incomingMessages = dataSource.FetchAll();
			Assert.IsTrue(incomingMessages.Count() == 1);
			var distributedMessage = incomingMessages.First();

			Assert.AreEqual(s1.SuscriptionHandlerId, distributedMessage.SuscriptionHandlerId);
			Assert.AreEqual(queueDispatcherManagerMessage.IncomingMessage, distributedMessage);

		}

		[Test, TestCaseSource(typeof (TestCaseSources), TestCaseSources.OptionDbInMemory)]
		public void When_Subscribed_ToConcrete_Type_Will_DistributeIt_Correctly(DbEngineType dbEngineType)
		{
			var factory = GetUnitOfWorkFactory(dbEngineType);
			var dataSource = GetRepository<Repository<IncomingMessage>>(factory);
			TransportMessage transportMessage1 =
				GetTransportMessage(new Dummy2() {TheValue = "Sample entity", TheValue2 = "2222"});

			var incomingMessage1 = new IncomingMessage(transportMessage1.Data)
				{
					ComponentOwner = RemoteComponentId,

					PublishedTo = LocalComponentId,
					TimeReceivedUtc = DateTime.UtcNow,
					SuscriptionHandlerId = Guid.Empty, //important as the p0ending messages have not subscriber yet
					Status = Message.MessageStatus.ReceiverReceived,
				};
			dataSource.Save(incomingMessage1);

			TransportMessage transportMessage2 = GetTransportMessage(new Dummy() {TheValue = "Sample entity2"});

			var incomingMessage2 = new IncomingMessage(transportMessage2.Data)
				{
					ComponentOwner = RemoteComponentId,

					PublishedTo = LocalComponentId,
					TimeReceivedUtc = DateTime.UtcNow,
					SuscriptionHandlerId = Guid.Empty, //important as the p0ending messages have not subscriber yet
					Status = Message.MessageStatus.ReceiverReceived,
				};
			dataSource.Save(incomingMessage2);

			IncomingMessageSuscription s1 = new IncomingMessageSuscription()
				{
					BizMessageFullTypeName = typeof (Dummy2).FullName,
					ComponentOwner = LocalComponentId,
					SuscriptionHandlerId = Guid.NewGuid(),
					HandlerType = typeof (string).FullName
				};
			var incomingMessageSuscriptionsDataSource = GetRepository<Repository<IncomingMessageSuscription>>(factory);
			incomingMessageSuscriptionsDataSource.Save(s1);

			IQueueDispatcherManager mockedDispatcher;
			using (var target = GetInstance(factory, DealWithMessage, out mockedDispatcher))
			{
				target.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage1));
				target.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage2));
				_messageReceived.WaitOne(TimeSpan.FromSeconds(10));
			}

			Assert.IsTrue(_sentMessages.Count == 1);
			// asserts the original was removed and only one Dmmy2 was distributed as there are not more subscribers
			var queueDispatcherManagerMessage = _sentMessages[0];
			Assert.IsTrue(queueDispatcherManagerMessage.MustCalculateLatency); //It was enqueued to recalculate latency
			var incomingMessages = dataSource.FetchAll();
			Assert.IsTrue(incomingMessages.Count() == 1);
			var distributedMessage = incomingMessages.First();

			Assert.AreEqual(s1.SuscriptionHandlerId, distributedMessage.SuscriptionHandlerId);
			Assert.AreEqual(queueDispatcherManagerMessage.IncomingMessage, distributedMessage);
		}
	}
}