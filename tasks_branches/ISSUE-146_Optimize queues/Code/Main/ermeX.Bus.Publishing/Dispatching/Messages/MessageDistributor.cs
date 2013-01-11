using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
    /// <summary>
    /// Distributes the messages with an entry per subscriber where the message havent been sent
    /// </summary>
    sealed class MessageDistributor:ProducerParallelConsumerQueue<MessageDistributor.MessageDistributorMessage>, IMessageDistributor
    {
        private const int _maxThreadsNum = 64;
        private const int _queueSizeToCreateNewThread = 4;
        private const int _initialWorkerCount = 1;

        public class MessageDistributorMessage{
            public OutgoingMessage OutGoingMessage { get; private set; }
            public BusMessage Message { get; private set; }

            public MessageDistributorMessage(OutgoingMessage outGoingMessage, BusMessage message)
            {

                if (outGoingMessage == null) throw new ArgumentNullException("outGoingMessage");
                if (message == null) throw new ArgumentNullException("message");
                OutGoingMessage = outGoingMessage;
                Message = message;
            }
        }

        [Inject]
        public MessageDistributor(IOutgoingMessageSuscriptionsDataSource subscriptionsDataSource,
            IOutgoingMessagesDataSource outgoingMessagesDataSource, IBusMessageDataSource busMessageDataSource,ISubscribersDispatcher dispatcher)
            : base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
        {
            if (subscriptionsDataSource == null) throw new ArgumentNullException("subscriptionsDataSource");
            if (outgoingMessagesDataSource == null) throw new ArgumentNullException("outgoingMessagesDataSource");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            SubscriptionsDataSource = subscriptionsDataSource;
            OutgoingMessagesDataSource = outgoingMessagesDataSource;
            BusMessageDataSource = busMessageDataSource;
            Dispatcher = dispatcher;
        }

        private IOutgoingMessageSuscriptionsDataSource SubscriptionsDataSource { get; set; }
        private IOutgoingMessagesDataSource OutgoingMessagesDataSource { get; set; }
        private IBusMessageDataSource BusMessageDataSource { get; set; }
        private ISubscribersDispatcher Dispatcher { get; set; }


        protected override Action<MessageDistributorMessage> RunActionOnDequeue
        {
            get
            {
                return OnDequeue;
                
            }
        }

        private void OnDequeue(MessageDistributorMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            //TODO: HIGHLY OPTIMIZABLE and THERe COULD BE CASES WHEN THE MESSAGE IS SENT TWICE
            var subscriptions = GetSubscriptions(message.Message.Data.MessageType.FullName);

            foreach (var messageSuscription in subscriptions)
            {
                if (OutgoingMessagesDataSource.ContainsMessageFor(message.Message.MessageId, messageSuscription.Component))
                    continue;
                
                var data = BusMessageDataSource.GetById(message.OutGoingMessage.BusMessageId);
                if (data == null) continue; //it could be expired
                
                var currentBusMessage = BusMessageData.NewFromExisting(data);
                currentBusMessage.Status = BusMessageData.BusMessageStatus.SenderDispatchPending;
                BusMessageDataSource.Save(currentBusMessage);

                var messageToSend = message.OutGoingMessage.GetClone();
                messageToSend.PublishedTo = messageSuscription.Component;
                messageToSend.BusMessageId = currentBusMessage.Id;
                OutgoingMessagesDataSource.Save(messageToSend);

                Dispatcher.EnqueueItem(new SubscribersDispatcher.SubscribersDispatcherMessage(messageToSend,currentBusMessage));
            }
        }

        private IEnumerable<OutgoingMessageSuscription> GetSubscriptions(string typeFullName)
        {
            var result = new List<OutgoingMessageSuscription>();
            var types = TypesHelper.GetInheritanceChain(typeFullName, true);

            foreach (var type in types)
            {
                var outgoingMessageSuscriptions = SubscriptionsDataSource.GetByMessageType(type.FullName);
                result.AddRange(outgoingMessageSuscriptions);
            }

            return result;
        }

    }
}
