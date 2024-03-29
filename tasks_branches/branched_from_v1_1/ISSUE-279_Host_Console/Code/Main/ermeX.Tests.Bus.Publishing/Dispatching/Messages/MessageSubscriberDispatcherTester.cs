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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Moq;
using NUnit.Framework;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Threading.Queues;
using ermeX.Threading.Scheduling;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Tests.Bus.Publishing.Dispatching.Messages
{
    class MessageSubscriberDispatcherTester : DataAccessTestBase
    {
        readonly IJobScheduler _taskScheduler=new JobScheduler();
        readonly List<TransportMessage> _sentMessages = new List<TransportMessage>();
        readonly ManualResetEvent _messageReceived=new ManualResetEvent(false);

        private MessageSubscribersDispatcher GetInstance(DbEngineType dbEngine, Action<TransportMessage> messageReceived,bool valueToReturn, out IServiceProxy mockedService)
        {
            var settings = TestSettingsProvider.GetClientConfigurationSettingsSource();
            var outgoingDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var mock = new Mock<IServiceProxy>();
            mock.Setup(x => x.Send(It.IsAny<TransportMessage>())).Callback(messageReceived).Returns(new ServiceResult(valueToReturn));
            mockedService = mock.Object;
            return new MessageSubscribersDispatcher(settings, outgoingDataSource, _taskScheduler, mockedService);
        }

        public override void OnStartUp()
        {
            base.OnStartUp();
            _sentMessages.Clear();
            _messageReceived.Reset();

        }

        private void DealWithMessage(TransportMessage message)
        {
            _sentMessages.Add(message);
            _messageReceived.Set();
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void SendsEnqueued_Items(DbEngineType dbEngine)
        {
            IServiceProxy mockedService;
            var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));

            using(var target=GetInstance(dbEngine,DealWithMessage, true,out mockedService))
            {
                var outgoingMessage = new OutgoingMessage(busMessage)
                {
                    CreatedTimeUtc = DateTime.UtcNow,
                    ComponentOwner = LocalComponentId,
                    PublishedBy = LocalComponentId,
                    PublishedTo = RemoteComponentId,
                    Status = Message.MessageStatus.SenderDispatchPending
                };
                var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
                outgoingMessagesDataSource.Save(outgoingMessage);

                target.EnqueueItem(new MessageSubscribersDispatcher.SubscribersDispatcherMessage(outgoingMessage));
                _messageReceived.WaitOne(TimeSpan.FromSeconds(5));
            }
            Assert.IsTrue(_sentMessages.Count==1,_sentMessages.Count.ToString(CultureInfo.InvariantCulture));
            var transportMessage = _sentMessages[0];
            Assert.AreEqual(busMessage,transportMessage.Data);

            
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void AfterSend_Items_MarksThemAsSent(DbEngineType dbEngine)
        {
            IServiceProxy mockedService;
            var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var outgoingMessage = new OutgoingMessage(busMessage)
            {
                CreatedTimeUtc = DateTime.UtcNow,
                ComponentOwner = LocalComponentId,
                PublishedBy = LocalComponentId,
                PublishedTo = RemoteComponentId,
                Status = Message.MessageStatus.SenderCollected  //to avoid reenqueue on startup
            };
            outgoingMessagesDataSource.Save(outgoingMessage);

            using (var target = GetInstance(dbEngine, DealWithMessage, true, out mockedService))
            {
                outgoingMessage.Status=Message.MessageStatus.SenderDispatchPending;
                outgoingMessagesDataSource.Save(outgoingMessage);

                target.EnqueueItem(new MessageSubscribersDispatcher.SubscribersDispatcherMessage(outgoingMessage));
                _messageReceived.WaitOne(TimeSpan.FromSeconds(20));
            }
            OutgoingMessage actual = outgoingMessagesDataSource.GetById(outgoingMessage.Id);
            Assert.AreEqual(Message.MessageStatus.SenderSent,actual.Status);
            
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void DontSend_Expired_Items(DbEngineType dbEngine)
        {
            IServiceProxy mockedService;
            var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var outgoingMessage = new OutgoingMessage(busMessage)
            {
                CreatedTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
                ComponentOwner = LocalComponentId,
                PublishedBy = LocalComponentId,
                PublishedTo = RemoteComponentId,
                Status = Message.MessageStatus.SenderCollected //to avoid reenqueue on startup
            };
            outgoingMessagesDataSource.Save(outgoingMessage);

            using (var target = GetInstance(dbEngine, DealWithMessage, true, out mockedService))
            {
                outgoingMessage.Status = Message.MessageStatus.SenderDispatchPending;
                outgoingMessagesDataSource.Save(outgoingMessage);

                target.EnqueueItem(new MessageSubscribersDispatcher.SubscribersDispatcherMessage(outgoingMessage));
                _messageReceived.WaitOne(TimeSpan.FromSeconds(5));
            }

            Assert.IsTrue(_sentMessages.Count == 0);
            OutgoingMessage actual = outgoingMessagesDataSource.GetById(outgoingMessage.Id);
            Assert.AreEqual(Message.MessageStatus.SenderFailed, actual.Status);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void WhenSend_Fails_Increases_Tries_And_ReEnqueuesItem(DbEngineType dbEngine)
        {
            IServiceProxy mockedService;
            var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var outgoingMessage = new OutgoingMessage(busMessage)
            {
                CreatedTimeUtc = DateTime.UtcNow,
                ComponentOwner = LocalComponentId,
                PublishedBy = LocalComponentId,
                PublishedTo = RemoteComponentId,
                Status = Message.MessageStatus.SenderCollected //to avoid reenqueue on startup
            };
            outgoingMessagesDataSource.Save(outgoingMessage);

            using (var target = GetInstance(dbEngine, DealWithMessage, false, out mockedService))
            {
                outgoingMessage.Status = Message.MessageStatus.SenderDispatchPending;
                outgoingMessagesDataSource.Save(outgoingMessage);

                target.EnqueueItem(new MessageSubscribersDispatcher.SubscribersDispatcherMessage(outgoingMessage));
                Thread.Sleep(TimeSpan.FromSeconds(17)); //resends 5 sec, 15 secs
            }
            OutgoingMessage actual = outgoingMessagesDataSource.GetById(outgoingMessage.Id);
            Assert.AreEqual(Message.MessageStatus.SenderDispatchPending, actual.Status);
            Assert.Greater(actual.Tries,2,actual.Tries.ToString(CultureInfo.InvariantCulture));

            Assert.IsTrue(_sentMessages.Count>2); //this probes that was reenqueued
        }


        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void RestoresPending_MessagesFrom_PreviousSessions_OnStartUp(DbEngineType dbEngine)
        {
            IServiceProxy mockedService;
            var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var outgoingMessage = new OutgoingMessage(busMessage)
            {
                CreatedTimeUtc = DateTime.UtcNow,
                ComponentOwner = LocalComponentId,
                PublishedBy = LocalComponentId,
                PublishedTo = RemoteComponentId,
                Status = Message.MessageStatus.SenderDispatchPending 
            };
            outgoingMessagesDataSource.Save(outgoingMessage);

            using (var target = GetInstance(dbEngine, DealWithMessage, true, out mockedService))
                _messageReceived.WaitOne(TimeSpan.FromSeconds(10));

            Assert.IsTrue(_sentMessages.Count == 1);
            OutgoingMessage actual = outgoingMessagesDataSource.GetById(outgoingMessage.Id);
            Assert.AreEqual(Message.MessageStatus.SenderSent, actual.Status);
        }
    }
}
