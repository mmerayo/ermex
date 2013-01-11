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
        private MessageDistributor GetInstance(DbEngineType dbEngine, Action<SubscribersDispatcher.SubscribersDispatcherMessage> messageReceived, out ISubscribersDispatcher mockedSubscriber)
        {
            var outgoingDataSource = GetDataSource<OutgoingMessagesDataSource>(dbEngine);
            var outgoingSubscriptionsDataSource = GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine);
            var mock = new Mock<ISubscribersDispatcher>();
            mock.Setup(x=>x.EnqueueItem(It.IsAny<SubscribersDispatcher.SubscribersDispatcherMessage>())).Callback(messageReceived);
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
            ISubscribersDispatcher mockedDispatcher;
            var actual = new List<SubscribersDispatcher.SubscribersDispatcherMessage>();
            
            var expected = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId, new BizMessage(new Dummy {Data="theData"}));

            //creates subscription
            var subscriptionsDs = GetDataSource<OutgoingMessageSuscriptionsDataSource>(dbEngine);
            var outgoingMessageSuscription = new OutgoingMessageSuscription()
                {
                    ComponentOwner = LocalComponentId, BizMessageFullTypeName = typeof (Dummy).FullName, Component = RemoteComponentId
                };
            subscriptionsDs.Save(outgoingMessageSuscription);

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

            Assert.IsTrue(actual.Count == 1); //ensure the message was delivered
            
            Assert.AreEqual(expected, actual[0].OutGoingMessage.ToBusMessage()); //ensure is the message that was sent
            var messagesInDb = outgoingMessagesDataSource.GetAll(); //ensures there are 2 messages the root one and the distributable one
            Assert.IsTrue(messagesInDb.Count==2); 

            var busMessage=  messagesInDb[1].ToBusMessage();//this one is the distributable
            Assert.IsTrue(messagesInDb[1].Status == Message.MessageStatus.SenderDispatchPending);
            Assert.AreEqual(expected,busMessage); //ensure is the same message
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void DontDispatchSentMessage(DbEngineType dbEngine)
        {
            ISubscribersDispatcher mockedDispatcher;
            var actual = new List<SubscribersDispatcher.SubscribersDispatcherMessage>();

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
