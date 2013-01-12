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
using Moq;
using NUnit.Framework;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Threading.Queues;

namespace ermeX.Tests.Bus.Publishing.Dispatching.Messages
{

    sealed class MessageDistributorTester : DataAccessTestBase
    {
        private readonly SystemTaskQueue _systemQueue = new SystemTaskQueue();
        private MessageDistributor GetInstance(DbEngineType dbEngine, Action<MessageSubscribersDispatcher.SubscribersDispatcherMessage> messageReceived, out IMessageSubscribersDispatcher mockedSubscriber)
        {
            var outgoingDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var outgoingSubscriptionsDataSource = GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine);
            var mock = new Mock<IMessageSubscribersDispatcher>();
            mock.Setup(x=>x.EnqueueItem(It.IsAny<MessageSubscribersDispatcher.SubscribersDispatcherMessage>())).Callback(messageReceived);
            mockedSubscriber = mock.Object;
            return new MessageDistributor(outgoingSubscriptionsDataSource,outgoingDataSource,mockedSubscriber,_systemQueue);
        }

        private class Dummy
        {
            public string Data;
        }
        

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanDispatch_NotSentMessage(DbEngineType dbEngine )
        {
            IMessageSubscribersDispatcher mockedDispatcher;
            var actual = new List<MessageSubscribersDispatcher.SubscribersDispatcherMessage>();
            
            var expected = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage(new Dummy {Data="theData"}));

            //creates subscription
            var subscriptionsDs = GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine);
            var suscription1 = new OutgoingMessageSuscription()
                {
                    ComponentOwner = LocalComponentId, BizMessageFullTypeName = typeof (Dummy).FullName, Component = RemoteComponentId
                };
            subscriptionsDs.Save(suscription1);
            var suscription2=new OutgoingMessageSuscription()
                {
                    ComponentOwner = LocalComponentId, BizMessageFullTypeName = typeof (Dummy).FullName, Component = Guid.NewGuid()
                };
            subscriptionsDs.Save(suscription2);
            //creates the message as collected
            //the default test set them for one day
            var outgoingMessage = new OutgoingMessage(expected)
                {
                    CreatedTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
                    ComponentOwner = LocalComponentId,
                    PublishedBy=LocalComponentId,
                    Status=Message.MessageStatus.SenderCollected
                };
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            outgoingMessagesDataSource.Save(outgoingMessage);

            //enqueues the message
            using (var target = GetInstance(dbEngine, actual.Add, out mockedDispatcher))
            {
                target.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outgoingMessage));
                while(actual.Count==0)
                    Thread.Sleep(50);

            }

            Assert.IsTrue(actual.Count == 2); //ensure the message was delivered

            OutgoingMessage outGoingMessage1 = actual[0].OutGoingMessage;
            Assert.AreEqual(expected, outGoingMessage1.ToBusMessage()); //ensure is the message that was sent
            Assert.AreEqual(suscription1.Component,outGoingMessage1.PublishedTo);

            OutgoingMessage outGoingMessage2 = actual[1].OutGoingMessage;
            Assert.AreEqual(expected, outGoingMessage2.ToBusMessage()); //ensure is the message that was sent
            Assert.AreEqual(suscription2.Component, outGoingMessage2.PublishedTo);

            var messagesInDb = outgoingMessagesDataSource.GetAll(); //ensures there are 3 messages the root one and the distributable ones
            Assert.IsTrue(messagesInDb.Count==3); 

            var busMessage=  messagesInDb[1].ToBusMessage();//this one is the distributable
            Assert.IsTrue(messagesInDb[1].Status == Message.MessageStatus.SenderDispatchPending);
            var busMessage2 = messagesInDb[2].ToBusMessage();//this one is the distributable
            Assert.IsTrue(messagesInDb[2].Status == Message.MessageStatus.SenderDispatchPending);
            Assert.AreEqual(expected,busMessage2); //ensure is the same message
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void DontDispatchSentMessage(DbEngineType dbEngine)
        {
            IMessageSubscribersDispatcher mockedDispatcher;
            var actual = new List<MessageSubscribersDispatcher.SubscribersDispatcherMessage>();

            var expected = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage(new Dummy { Data = "theData" }));

            //creates subscription
            var subscriptionsDs = GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine);
            var outgoingMessageSuscription = new OutgoingMessageSuscription()
            {
                ComponentOwner = LocalComponentId,
                BizMessageFullTypeName = typeof(Dummy).FullName,
                Component = RemoteComponentId
            };
            subscriptionsDs.Save(outgoingMessageSuscription);

            //creates the message as collected
            //the default test set them for one day
            var outgoingMessage = new OutgoingMessage(expected)
            {
                CreatedTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
                ComponentOwner = LocalComponentId,
                PublishedBy = LocalComponentId,
                Status = Message.MessageStatus.SenderCollected
            };
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            outgoingMessagesDataSource.Save(outgoingMessage); //saves one
            
            
            //enqueues the first message pretending it needs to be sent
            using (var target = GetInstance(dbEngine, actual.Add, out mockedDispatcher))
            {
                for (int i = 0; i < 500;i++ )
                {
                    //pushes it several times
                    target.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outgoingMessage));
                }
                Thread.Sleep(50);
            }

            Assert.AreEqual(1,actual.Count); //Assert only one was sent
            var outgoingMessages = outgoingMessagesDataSource.GetByStatus(Message.MessageStatus.SenderDispatchPending);
            Assert.IsTrue(outgoingMessages.Count()==1,outgoingMessages.Count().ToString(CultureInfo.InvariantCulture)); //Asserts the second one was not considered and removed

            OutgoingMessage pushedMessage = actual[0].OutGoingMessage;
            Assert.IsTrue( pushedMessage.Status==Message.MessageStatus.SenderDispatchPending);
            Assert.AreEqual(outgoingMessage.ToBusMessage(),pushedMessage.ToBusMessage());

        }

    }
}
