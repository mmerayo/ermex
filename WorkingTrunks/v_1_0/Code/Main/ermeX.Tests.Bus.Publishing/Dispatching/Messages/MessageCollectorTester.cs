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
using ermeX.DAL.DataAccess.DataSources;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Threading.Queues;

namespace ermeX.Tests.Bus.Publishing.Dispatching.Messages
{

    sealed class MessageCollectorTester : DataAccessTestBase
    {
        private readonly SystemTaskQueue _systemQueue=new SystemTaskQueue();

        private MessageCollector GetInstance(DbEngineType dbEngine,Action<MessageDistributor.MessageDistributorMessage> messageReceived, out IMessageDistributor mockedDistributor)
        {
            var settings = TestSettingsProvider.GetClientConfigurationSettingsSource();
            var outgoingDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var mock = new Mock<IMessageDistributor>();
            mock.Setup(x=>x.EnqueueItem(It.IsAny<MessageDistributor.MessageDistributorMessage>())).Callback(messageReceived);
            mockedDistributor = mock.Object;
            return new MessageCollector(settings,  _systemQueue, outgoingDataSource,mockedDistributor);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void ComponentStopsOnDisposal(DbEngineType dbEngine)
        {
            IMessageDistributor mockedDistributor;
            var actual = new List<MessageDistributor.MessageDistributorMessage>();
            var target = GetInstance(dbEngine, actual.Add, out mockedDistributor);
            target.Start();
            target.Dispose();
            Assert.AreEqual(DispatcherStatus.Stopped,  target.Status);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanDispatchMessage(DbEngineType dbEngine )
        {
            IMessageDistributor mockedDistributor;
            var actual = new List<MessageDistributor.MessageDistributorMessage>();
            var expected = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId,
                                         new BizMessage("theData"));
            using (var target = GetInstance(dbEngine, actual.Add, out mockedDistributor))
            {
                target.Start();
                target.Dispatch(expected);
                while(actual.Count==0)
                    Thread.Sleep(50);

            }

            Assert.IsTrue(actual.Count==1);
            BusMessage busMessage = actual[0].OutGoingMessage.ToBusMessage();
            Assert.AreEqual(expected, busMessage); //asserts is the same that was pushed
            var messagesInDb= GetDataSource<OutgoingMessagesDataSource>(dbEngine).GetAll();
            Assert.IsTrue(messagesInDb.Count==1);  //asserts the message was created

            OutgoingMessage outgoingMessage = messagesInDb[0];
            Assert.IsTrue(outgoingMessage.Status==Message.MessageStatus.SenderCollected);
            Assert.AreEqual(expected,outgoingMessage.ToBusMessage()); //asserts is the same pushed one
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void RemovesExpiredItems(DbEngineType dbEngine)
        {
            
            var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));

            //the default test set them for one day
            var outgoingMessage = new OutgoingMessage(busMessage)
                {
                    CreatedTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
                    ComponentOwner = LocalComponentId,
                    PublishedBy=LocalComponentId,
                    Status=Message.MessageStatus.SenderCollected
                };
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            outgoingMessagesDataSource.Save(outgoingMessage);

            Assert.IsTrue(outgoingMessagesDataSource.GetAll().Count==1); //assert it was saved

            var actual = new List<MessageDistributor.MessageDistributorMessage>();
            IMessageDistributor mockedDistributor;
            using (var target = GetInstance(dbEngine, actual.Add, out mockedDistributor))
            {
                target.Start();
                Thread.Sleep(250);
            }

            Assert.IsTrue(outgoingMessagesDataSource.GetAll().Count == 0);//asserts it was removed
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void SendsExistingItemsOnStart(DbEngineType dbEngine)
        {

            var busMessage = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage("theData"));

            //the default test set them for one day
            var outgoingMessage = new OutgoingMessage(busMessage)//creates this message as a pending one
            {
                CreatedTimeUtc = DateTime.UtcNow,
                ComponentOwner = LocalComponentId,
                PublishedBy = LocalComponentId,
                Status = Message.MessageStatus.SenderCollected
            };
            var outgoingMessagesDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            outgoingMessagesDataSource.Save(outgoingMessage);  

            IMessageDistributor mockedDistributor;
            var actual = new List<MessageDistributor.MessageDistributorMessage>();
            using (var target = GetInstance(dbEngine, actual.Add, out mockedDistributor))
            {
                target.Start();
                while (actual.Count == 0)
                    Thread.Sleep(50);

            }

            Assert.IsTrue(actual.Count == 1);  //asserts it was sent
            OutgoingMessage actualOutgoingMessage = actual[0].OutGoingMessage;
            Assert.AreEqual(outgoingMessage, actualOutgoingMessage);
            BusMessage message = actualOutgoingMessage.ToBusMessage();
            Assert.AreEqual(busMessage, message);
            
        }

    }
}
