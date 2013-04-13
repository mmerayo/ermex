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
using System.Threading;
using Moq;
using NUnit.Framework;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Threading.Queues;
using ermeX.Threading.Scheduling;

namespace ermeX.Tests.Bus.Listening.Handlers.InternalMessageHandling.WorkflowHandlers
{
	internal class QueueDispatcherManagerTester : DataAccessTestBase
	{

		private readonly List<Dummy> _sentMessages = new List<Dummy>();
		private int _flagMessageReceivedWhentimes;
		private readonly ManualResetEvent _messageReceived = new ManualResetEvent(false);

		private QueueDispatcherManager GetInstance(IUnitOfWorkFactory factory, Action<Guid, object> deliveryHandler)
		{
			var settings = TestSettingsProvider.GetClientConfigurationSettingsSource();

			var result = new QueueDispatcherManager(settings, GetIncommingQueueWritter(factory), GetLatenciesReader(factory),
			                                        GetLatenciesWritter(factory));
			result.DispatchMessage += deliveryHandler;
			return result;
		}

		public override void OnStartUp()
		{
			base.OnStartUp();
			_sentMessages.Clear();
			_messageReceived.Reset();
			_calls = 0;
			_flagMessageReceivedWhentimes = 1;

		}

		private void DealWithMessage(Guid subscriptionHandlerId, object message)
		{
			_sentMessages.Add((Dummy) message);
			if (Interlocked.Decrement(ref _flagMessageReceivedWhentimes) == 0)
				_messageReceived.Set();
		}

		private int _calls = 0;

		private void DealWithMessageFailingOnce(Guid subscriptionHandlerId, object message)
		{
			if (Interlocked.Increment(ref _calls) == 1)
				throw new Exception("Test");
			DealWithMessage(subscriptionHandlerId, message);
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
			public int TheOrder { get; set; }
		}



		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void Can_Dispatch_Messages(DbEngineType dbEngineType)
		{
			IUnitOfWorkFactory factory = GetUnitOfWorkFactory(dbEngineType);

			var dataSource = GetRepository<Repository<IncomingMessage>>();
			var dummy = new Dummy() {TheValue = "Sample entity"};
			TransportMessage transportMessage = GetTransportMessage(dummy);

			TimeSpan createdTimeDelay = TimeSpan.FromSeconds(3);
			var incomingMessage = new IncomingMessage(transportMessage.Data)
			                      	{
			                      		ComponentOwner = RemoteComponentId,

			                      		PublishedTo = LocalComponentId,
			                      		TimeReceivedUtc = DateTime.UtcNow,
			                      		CreatedTimeUtc = DateTime.UtcNow.Subtract(createdTimeDelay),
			                      		SuscriptionHandlerId = Guid.NewGuid(),
			                      		//not relevant
			                      		Status = Message.MessageStatus.ReceiverDispatchable,
			                      	};
			using (var unitOfWork = factory.Create())
			{
				dataSource.Save(unitOfWork, incomingMessage);
				unitOfWork.Commit();
			}

			using (var target = GetInstance(factory, DealWithMessage))
			{
				target.EnqueueItem(new QueueDispatcherManager.QueueDispatcherManagerMessage(incomingMessage, true));
				_messageReceived.WaitOne(TimeSpan.FromSeconds(5));
			}

			Assert.IsTrue(_sentMessages.Count == 1);
			Assert.AreEqual(dummy.TheValue, _sentMessages[0].TheValue);

			Assert.IsTrue(incomingMessage.Id > 0); //now check it is removed
			using (var unitOfWork = factory.Create())
			{
				var byId = dataSource.SingleOrDefault(unitOfWork, incomingMessage.Id);
				unitOfWork.Commit();
				Assert.IsNull(byId);
			}
		}

		[Ignore("Its covered implicitly in other test cases")]
		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void Removes_Dispatched_Messages(DbEngineType dbEngineType)
		{
			throw new NotImplementedException();
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void Retries_Failed_Messages(DbEngineType dbEngineType)
		{
			IUnitOfWorkFactory factory = GetUnitOfWorkFactory(dbEngineType);
			var dataSource = GetRepository<Repository<IncomingMessage>>();
			var dummy = new Dummy() {TheValue = "Sample entity"};
			TransportMessage transportMessage = GetTransportMessage(dummy);

			TimeSpan createdTimeDelay = TimeSpan.FromSeconds(3);
			var incomingMessage = new IncomingMessage(transportMessage.Data)
			                      	{
			                      		ComponentOwner = RemoteComponentId,

			                      		PublishedTo = LocalComponentId,
			                      		TimeReceivedUtc = DateTime.UtcNow,
			                      		CreatedTimeUtc = DateTime.UtcNow.Subtract(createdTimeDelay),
			                      		SuscriptionHandlerId = Guid.NewGuid(),
			                      		//not relevant
			                      		Status = Message.MessageStatus.ReceiverDispatchable,
			                      	};
			using (var unitOfWork = factory.Create())
			{
				dataSource.Save(unitOfWork,incomingMessage);
				unitOfWork.Commit();
			}
			using (var target = GetInstance(factory, DealWithMessageFailingOnce))
			{
				target.EnqueueItem(new QueueDispatcherManager.QueueDispatcherManagerMessage(incomingMessage, true));
				_messageReceived.WaitOne(TimeSpan.FromSeconds(60));
			}

			Assert.IsTrue(_calls == 2, "_calls:" + _calls.ToString(CultureInfo.InvariantCulture));

			Assert.IsTrue(_sentMessages.Count == 1);
			Assert.AreEqual(dummy.TheValue, _sentMessages[0].TheValue);

			Assert.IsTrue(incomingMessage.Id > 0); //now check it is removed

			IncomingMessage byId = dataSource.Single(incomingMessage.Id);
			Assert.IsNull(byId);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void Can_DeliverMany_Messages_OrderedByGeneration(DbEngineType dbEngineType)
		{
			IUnitOfWorkFactory factory = GetUnitOfWorkFactory(dbEngineType);
			var dataSource = GetRepository<Repository<IncomingMessage>>();


			TimeSpan createdTimeDelay = TimeSpan.FromSeconds(3);
			const int numMessages = 50;
			var messages = new List<IncomingMessage>(numMessages);
			for (int i = 0; i < numMessages; i++)
			{
				var dummy = new Dummy() {TheValue = "Sample entity", TheOrder = i};
				TransportMessage transportMessage = GetTransportMessage(dummy);

				var incomingMessage = new IncomingMessage(transportMessage.Data)
				                      	{
				                      		ComponentOwner = RemoteComponentId,

				                      		PublishedTo = LocalComponentId,
				                      		TimeReceivedUtc = DateTime.UtcNow,
				                      		CreatedTimeUtc = DateTime.UtcNow.Subtract(createdTimeDelay),
				                      		SuscriptionHandlerId = Guid.NewGuid(),
				                      		//not relevant
				                      		Status = Message.MessageStatus.ReceiverDispatchable,
				                      	};
				dataSource.Save(incomingMessage);
				messages.Add(incomingMessage);
				Thread.Sleep(1); //to ensure the createdtimeutc is different
			}

			_flagMessageReceivedWhentimes = numMessages; //flag the event when received


			using (var target = GetInstance(factory, DealWithMessage))
			{
				foreach (var incomingMessage in messages)
				{
					target.EnqueueItem(new QueueDispatcherManager.QueueDispatcherManagerMessage(incomingMessage, true));
				}

				_messageReceived.WaitOne(TimeSpan.FromSeconds(120));
			}

			Assert.IsTrue(_sentMessages.Count == numMessages, "SentMessages:" + _sentMessages.Count);

			//check order
			var previous = -1;
			foreach (var sentMessage in _sentMessages)
			{
				Assert.IsTrue(sentMessage.TheOrder > previous,
				              string.Format("SentMessage: {0}  Previous: {1}", sentMessage.TheOrder, previous));
				previous = sentMessage.TheOrder;
			}

			Assert.IsFalse(dataSource.Any());
		}

		[Ignore("This class will be upgraded ASAP. Its not worth to test more with the current resources")]
		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void Can_DeliverRespecting_Latency(DbEngineType dbEngineType)
		{
			throw new NotImplementedException();
		}

		[Ignore("This class will be upgraded ASAP. Its not worth to test more with the current resources")]
		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void When_Disposed_Removes_ScheduledJobs(DbEngineType dbEngineType)
		{
			throw new NotImplementedException();
		}
	}
}