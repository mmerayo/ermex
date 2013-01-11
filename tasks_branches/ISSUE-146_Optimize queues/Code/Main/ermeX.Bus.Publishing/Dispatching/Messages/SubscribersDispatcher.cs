using System;
using Ninject;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
    /// <summary>
    /// Distributes the messages with an entry per subscriber where the message havent been sent
    /// </summary>
    sealed class SubscribersDispatcher : ProducerParallelConsumerQueue<SubscribersDispatcher.SubscribersDispatcherMessage>, ISubscribersDispatcher
    {
        private const int _maxThreadsNum = 64;
        private const int _queueSizeToCreateNewThread = 4;
        private const int _initialWorkerCount = 4;

        public class SubscribersDispatcherMessage{
            public OutgoingMessage OutGoingMessage { get; private set; }

            public SubscribersDispatcherMessage(OutgoingMessage outGoingMessage)
            {
                if (outGoingMessage == null) throw new ArgumentNullException("outGoingMessage");
                OutGoingMessage = outGoingMessage;
            }
        }

        [Inject]
        public SubscribersDispatcher()
            : base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
        {
        }


        protected override Action<SubscribersDispatcherMessage> RunActionOnDequeue
        {
            get
            {
                throw new NotImplementedException();        
            }
        }


        
    }
}