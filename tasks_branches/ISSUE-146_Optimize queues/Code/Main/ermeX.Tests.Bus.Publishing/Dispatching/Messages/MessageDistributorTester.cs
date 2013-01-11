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

    sealed class MessageDistributorTester : DataAccessTestBase
    {
        private MessageDistributor GetInstance(DbEngineType dbEngine, Action<SubscribersDispatcher.SubscribersDispatcherMessage> messageReceived, out ISubscribersDispatcher mockedSubscriber)
        {
            var busMessageDataSource = GetBusMessageDataSource(dbEngine);
            var outgoingDataSource = GetOutgoingMessageDataSource(dbEngine);
            var outgoingSubscriptionsDataSource = GetOutgoingMessageSubscriptionsDataSource(dbEngine);
            var mock = new Mock<ISubscribersDispatcher>();
            mock.Setup(x=>x.EnqueueItem(It.IsAny<SubscribersDispatcher.SubscribersDispatcherMessage>())).Callback(messageReceived);
            mockedSubscriber = mock.Object;
            return new MessageDistributor(outgoingSubscriptionsDataSource,outgoingDataSource,busMessageDataSource,mockedSubscriber);
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
            var busMessageData = BusMessageData.FromBusLayerMessage(LocalComponentId, expected, BusMessageData.BusMessageStatus.SenderCollected);

            var subscriptionsDs = GetOutgoingMessageSubscriptionsDataSource(dbEngine);
            var outgoingMessageSuscription = new OutgoingMessageSuscription()
                {
                    ComponentOwner = LocalComponentId, BizMessageFullTypeName = typeof (Dummy).FullName, Component = RemoteComponentId
                };
            subscriptionsDs.Save(outgoingMessageSuscription);

            BusMessageDataSource busMessageDataSource = GetBusMessageDataSource(dbEngine);
            busMessageDataSource.Save(busMessageData);
            //the default test set them for one day
            var outgoingMessage = new OutgoingMessage(busMessageData)
                {
                    TimePublishedUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)),
                    ComponentOwner = LocalComponentId,
                    PublishedBy=LocalComponentId
                };
            OutgoingMessagesDataSource outgoingMessagesDataSource = GetOutgoingMessageDataSource(dbEngine);
            outgoingMessagesDataSource.Save(outgoingMessage);

            using (var target = GetInstance(dbEngine, actual.Add, out mockedDispatcher))
            {
                target.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outgoingMessage,expected));
                while(actual.Count==0)
                    Thread.Sleep(50);

            }

            Assert.IsTrue(actual.Count==1);
            Assert.AreSame(expected, actual[0].Message);
            var messagesInDb= GetOutgoingMessages(dbEngine);
            Assert.IsTrue(messagesInDb.Count==2);

            var busMessage=  GetBusMessage(dbEngine, messagesInDb[1].BusMessageId);
            Assert.IsTrue(busMessage.Status==BusMessageData.BusMessageStatus.SenderDispatchPending);
            Assert.AreEqual(expected,(BusMessage)busMessage);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void DontDispatchSentMessage(DbEngineType dbEngine)
        {
            throw new NotImplementedException();
        }

    }
}
