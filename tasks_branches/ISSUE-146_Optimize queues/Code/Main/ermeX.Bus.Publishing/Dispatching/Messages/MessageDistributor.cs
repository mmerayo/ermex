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
    /// Distributes the messages creating an entry per subscriber where the message havent been sent
    /// </summary>
    sealed class MessageDistributor:ProducerParallelConsumerQueue<MessageDistributor.MessageDistributorMessage>, IMessageDistributor
    {
        private const int _maxThreadsNum = 128;
        private const int _queueSizeToCreateNewThread = 4;
        private const int _initialWorkerCount = 1;

        public class MessageDistributorMessage{
            public OutgoingMessage OutGoingMessage { get; private set; }

            public MessageDistributorMessage(OutgoingMessage outGoingMessage)
            {

                if (outGoingMessage == null) throw new ArgumentNullException("outGoingMessage");
                OutGoingMessage = outGoingMessage;
            }
        }

        [Inject]
        public MessageDistributor(IOutgoingMessageSuscriptionsDataSource subscriptionsDataSource,
            IOutgoingMessagesDataSource outgoingMessagesDataSource, ISubscribersDispatcher dispatcher, SystemTaskQueue taskQueue)
            : base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
        {
            if (subscriptionsDataSource == null) throw new ArgumentNullException("subscriptionsDataSource");
            if (outgoingMessagesDataSource == null) throw new ArgumentNullException("outgoingMessagesDataSource");
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (taskQueue == null) throw new ArgumentNullException("taskQueue");
            SubscriptionsDataSource = subscriptionsDataSource;
            OutgoingMessagesDataSource = outgoingMessagesDataSource;
            Dispatcher = dispatcher;
            TaskQueue = taskQueue;
        }

        private IOutgoingMessageSuscriptionsDataSource SubscriptionsDataSource { get; set; }
        private IOutgoingMessagesDataSource OutgoingMessagesDataSource { get; set; }
        private ISubscribersDispatcher Dispatcher { get; set; }
        private SystemTaskQueue TaskQueue { get; set; }


        protected override Action<MessageDistributorMessage> RunActionOnDequeue
        {
            get
            {
                return OnDequeue;
                
            }
        }

        private readonly Dictionary<Guid,object> _subscriptorLockers=new Dictionary<Guid, object>(); 

        private void OnDequeue(MessageDistributorMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            //TODO: HIGHLY OPTIMIZABLE and THERe COULD BE CASES WHEN THE MESSAGE IS SENT TWICE
            OutgoingMessage outGoingMessage = message.OutGoingMessage;
            BusMessage busMessage = outGoingMessage.ToBusMessage();

            var subscriptions = GetSubscriptions(busMessage.Data.MessageType.FullName);

            foreach (var messageSuscription in subscriptions)
            {
                Guid destinationComponent = messageSuscription.Component;
                if (!_subscriptorLockers.ContainsKey(destinationComponent))
                    lock (_subscriptorLockers)
                        if (!_subscriptorLockers.ContainsKey(destinationComponent))
                            _subscriptorLockers.Add(destinationComponent, new object());

                object subscriptorLocker = _subscriptorLockers[destinationComponent];
                lock (subscriptorLocker) // its sequential by component
                {
                    //ensures it was not sent before this is not atomical because it will only happen when restarting or another component reconnecting
                    if (OutgoingMessagesDataSource.ContainsMessageFor(outGoingMessage.MessageId, destinationComponent))
                        continue;

                    var messageToSend = outGoingMessage.GetClone(); //creates a copy for the subscriber
                    messageToSend.Status = Message.MessageStatus.SenderDispatchPending; //ready to be dispatched
                    messageToSend.PublishedTo = destinationComponent; //assigns the receiver
                    Dispatcher.EnqueueItem(new SubscribersDispatcher.SubscribersDispatcherMessage(messageToSend));//pushes it
                    
                    OutgoingMessagesDataSource.Save(messageToSend);//update the db
                    
                }
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
