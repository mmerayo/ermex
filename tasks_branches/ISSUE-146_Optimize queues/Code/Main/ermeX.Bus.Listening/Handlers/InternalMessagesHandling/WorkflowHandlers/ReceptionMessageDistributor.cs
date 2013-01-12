using System;
using System.Collections.Generic;
using Ninject;
using ermeX.Common;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    internal class ReceptionMessageDistributor: ProducerParallelConsumerQueue<ReceptionMessageDistributor.MessageDistributorMessage>, IReceptionMessageDistributor
    {
        private const int _maxThreadsNum = 128;
        private const int _queueSizeToCreateNewThread = 4;
        private const int _initialWorkerCount = 1;
        public class MessageDistributorMessage
        {
            public IncomingMessage IncomingMessage { get; private set; }

            public MessageDistributorMessage(IncomingMessage message)
            {
                if (message == null) throw new ArgumentNullException("message");
                IncomingMessage = message;
            }
        }

        [Inject]
        public ReceptionMessageDistributor(IIncomingMessageSuscriptionsDataSource subscriptionsDataSource,
                                  IIncomingMessagesDataSource messagesDataSource, IQueueDispatcherManager dispatcher, SystemTaskQueue taskQueue)
            : base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
        {
            if (subscriptionsDataSource == null) throw new ArgumentNullException("subscriptionsDataSource");
            if (messagesDataSource == null) throw new ArgumentNullException("messagesDataSource");
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (taskQueue == null) throw new ArgumentNullException("taskQueue");
            SubscriptionsDataSource = subscriptionsDataSource;
            MessagesDataSource = messagesDataSource;
            Dispatcher = dispatcher;
            TaskQueue = taskQueue;
        }

        private IIncomingMessageSuscriptionsDataSource SubscriptionsDataSource { get; set; }
        private IIncomingMessagesDataSource MessagesDataSource { get; set; }
        private IQueueDispatcherManager Dispatcher { get; set; }
        private SystemTaskQueue TaskQueue { get; set; }


        protected override Action<MessageDistributorMessage> RunActionOnDequeue
        {
            get
            {
                return OnDequeue;
            }
        }

        private readonly Dictionary<Guid, object> _subscriptorLockers = new Dictionary<Guid, object>();

        private void OnDequeue(MessageDistributorMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            //TODO: HIGHLY OPTIMIZABLE and THERe COULD BE CASES WHEN THE MESSAGE IS SENT TWICE
            var incomingMessage = message.IncomingMessage;
            BusMessage busMessage = incomingMessage.ToBusMessage();

            var subscriptions = GetSubscriptions(busMessage.Data.MessageType.FullName);

            foreach (var messageSuscription in subscriptions)
            {
                Guid destination = messageSuscription.SuscriptionHandlerId; //TODO: ISSUE-244--> IT GOES TO THE QUEUE AND THE QUEUE KEEPS THE HANDLERS
                if (!_subscriptorLockers.ContainsKey(destination))
                    lock (_subscriptorLockers)
                        if (!_subscriptorLockers.ContainsKey(destination))
                            _subscriptorLockers.Add(destination, new object());

                object subscriptorLocker = _subscriptorLockers[destination];
                lock (subscriptorLocker) // its sequential by component
                {
                    //ensures it was not sent before this is not atomical because it will only happen when restarting or another component reconnecting
                    if (MessagesDataSource.ContainsMessageFor(incomingMessage.MessageId, destination))
                        continue;

                    var messageToDeliver = incomingMessage.GetClone(); //creates a copy for the subscriber
                    messageToDeliver.Status = Message.MessageStatus.ReceiverDispatchable; //ready to be dispatched
                    messageToDeliver.SuscriptionHandlerId = destination;
                    Dispatcher.EnqueueItem(new QueueDispatcherManager.QueueDispatcherManagerMessage(messageToDeliver));//pushes it

                    MessagesDataSource.Save(messageToDeliver);//update the db
                }
            }
        }

        private IEnumerable<IncomingMessageSuscription> GetSubscriptions(string typeFullName)
        {
            var result = new List<IncomingMessageSuscription>();
            var types = TypesHelper.GetInheritanceChain(typeFullName, true);

            foreach (var type in types)
            {
                result.AddRange(SubscriptionsDataSource.GetByMessageType(type.FullName));
            }

            return result;
        }

        

    }
}