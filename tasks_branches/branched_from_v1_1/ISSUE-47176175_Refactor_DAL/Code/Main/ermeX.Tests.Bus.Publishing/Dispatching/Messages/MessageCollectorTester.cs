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
using ermeX.Bus.Interfaces;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Observer;
using ermeX.DAL.Interfaces.Observers;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Threading.Queues;

namespace ermeX.Tests.Bus.Publishing.Dispatching.Messages
{
	internal sealed class MessageCollectorTester : DataAccessTestBase
	{
		private readonly List<MessageDistributor.MessageDistributorMessage> _sentMessages =
			new List<MessageDistributor.MessageDistributorMessage>();

		private readonly ManualResetEvent _messageReceived = new ManualResetEvent(false);

		private MessageCollector GetInstance(IUnitOfWorkFactory factory,
		                                     Action<MessageDistributor.MessageDistributorMessage> messageReceived,
		                                     out IMessageDistributor mockedDistributor)
		{
			var settings = TestSettingsProvider.GetClientConfigurationSettingsSource();
			var mock = new Mock<IMessageDistributor>();
			mock.Setup(x => x.EnqueueItem(It.IsAny<MessageDistributor.MessageDistributorMessage>())).Callback(messageReceived);
			mockedDistributor = mock.Object;
			return new MessageCollector(settings, mockedDistributor,
			                            GetOutgoingQueueReader(factory),
			                            GetOutgoingQueueWritter(factory),
			                            GetDomainNotifier());

		}

		public override void OnStartUp()
		{
			base.OnStartUp();
			_sentMessages.Clear();
			_messageReceived.Reset();

		}

		private void DealWithMessage(MessageDistributor.MessageDistributorMessage message)
		{
			_sentMessages.Add(message);
			_messageReceived.Set();
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void ComponentStopsOnDisposal(DbEngineType dbEngine)
		{
			IMessageDistributor mockedDistributor;
			IUnitOfWorkFactory factory = GetUnitOfWorkFactory(dbEngine);
			var target = GetInstance(factory, DealWithMessage, out mockedDistributor);
			target.Start();
			target.Dispose();
			Assert.AreEqual(DispatcherStatus.Stopped, target.Status);
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void CanDispatchMessage(DbEngineType dbEngine)
		{
			IMessageDistributor mockedDistributor;
			var expected = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId,
			                              new BizMessage("theData"));
			IUnitOfWorkFactory factory = GetUnitOfWorkFactory(dbEngine);
			using (var target = GetInstance(factory, DealWithMessage, out mockedDistributor))
			{
				target.Start();
				target.Dispatch(expected);
				_messageReceived.WaitOne(TimeSpan.FromSeconds(10));

			}

			Assert.IsTrue(_sentMessages.Count == 1);
			BusMessage busMessage = _sentMessages[0].OutGoingMessage.ToBusMessage();
			Assert.AreEqual(expected, busMessage); //asserts is the same that was pushed
			List<OutgoingMessage> messagesInDb;
			messagesInDb = GetRepository<Repository<OutgoingMessage>>(factory).FetchAll().ToList();
			Assert.IsTrue(messagesInDb.Count() == 1); //asserts the message was created

			OutgoingMessage outgoingMessage = messagesInDb.First();
			Assert.IsTrue(outgoingMessage.Status == Message.MessageStatus.SenderCollected);
			Assert.AreEqual(expected, outgoingMessage.ToBusMessage()); //asserts is the same pushed one
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void RemovesExpiredItems(DbEngineType dbEngine)
		{

			var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));

			//the default test set them for one day
			var outgoingMessage = new OutgoingMessage(busMessage)
			                      	{
			                      		CreatedTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
			                      		ComponentOwner = LocalComponentId,
			                      		PublishedBy = LocalComponentId,
			                      		Status = Message.MessageStatus.SenderCollected
			                      	};
			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(dbEngine);
			var outgoingMessagesDataSource = GetRepository<Repository<OutgoingMessage>>(unitOfWorkFactory);
			outgoingMessagesDataSource.Save(outgoingMessage);

			int count = outgoingMessagesDataSource.Count();
			Assert.IsTrue(count == 1); //assert it was saved
			IMessageDistributor mockedDistributor;
			using (var target = GetInstance(unitOfWorkFactory, DealWithMessage, out mockedDistributor))
			{
				target.Start();
				Thread.Sleep(250);
			}

			Assert.IsTrue(!outgoingMessagesDataSource.Any()); //asserts it was removed
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void SendsExistingItemsOnStart(DbEngineType dbEngine)
		{

			var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));

			//the default test set them for one day
			var outgoingMessage = new OutgoingMessage(busMessage) //creates this message as a pending one
			                      	{
			                      		CreatedTimeUtc = DateTime.UtcNow,
			                      		ComponentOwner = LocalComponentId,
			                      		PublishedBy = LocalComponentId,
			                      		Status = Message.MessageStatus.SenderCollected
			                      	};
			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(dbEngine);
			var outgoingMessagesDataSource = GetRepository<Repository<OutgoingMessage>>(unitOfWorkFactory);

			outgoingMessagesDataSource.Save(outgoingMessage);


			IMessageDistributor mockedDistributor;
			using (var target = GetInstance(unitOfWorkFactory, DealWithMessage, out mockedDistributor))
			{
				target.Start();
				_messageReceived.WaitOne(TimeSpan.FromSeconds(10));

			}

			Assert.IsTrue(_sentMessages.Count == 1); //asserts it was sent
			OutgoingMessage actualOutgoingMessage = _sentMessages[0].OutGoingMessage;
			Assert.AreEqual(outgoingMessage, actualOutgoingMessage);
			BusMessage message = actualOutgoingMessage.ToBusMessage();
			Assert.AreEqual(busMessage, message);

		}


		private class SomeData
		{
			public string TheData { get; set; }
		}

		[Test, TestCaseSource(typeof (TestCaseSources), "InMemoryDb")]
		public void SendsExistingItemsOnSubscriptionReceived(DbEngineType dbEngine)
		{

			var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId,
			                                new BizMessage(new SomeData() {TheData = "theData"}));
			IMessageDistributor mockedDistributor;
			IUnitOfWorkFactory unitOfWorkFactory = GetUnitOfWorkFactory(dbEngine);
			using (var target = GetInstance(unitOfWorkFactory, DealWithMessage, out mockedDistributor))
			{
				target.Start();
				//the default test set them for one day
				var outgoingMessage = new OutgoingMessage(busMessage) //creates this message as a pending one
				                      	{
				                      		CreatedTimeUtc = DateTime.UtcNow,
				                      		ComponentOwner = LocalComponentId,
				                      		PublishedBy = LocalComponentId,
				                      		Status = Message.MessageStatus.SenderCollected
				                      	};
				var outgoingMessagesDataSource = GetRepository<Repository<OutgoingMessage>>(unitOfWorkFactory);
				outgoingMessagesDataSource.Save(outgoingMessage);

				target.Notify(ObservableAction.Add, new OutgoingMessageSuscription()
				                                    	{
				                                    		BizMessageFullTypeName = typeof (SomeData).FullName,
				                                    		Component = LocalComponentId,
				                                    		ComponentOwner = LocalComponentId,
				                                    		DateLastUpdateUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
				                                    	});

				_messageReceived.WaitOne(TimeSpan.FromSeconds(10));

				Assert.IsTrue(_sentMessages.Count == 1); //asserts it was sent
				OutgoingMessage actualOutgoingMessage = _sentMessages[0].OutGoingMessage;
				Assert.AreEqual(outgoingMessage, actualOutgoingMessage);
				BusMessage message = actualOutgoingMessage.ToBusMessage();
				Assert.AreEqual(busMessage, message);
			}
		}
	}
}