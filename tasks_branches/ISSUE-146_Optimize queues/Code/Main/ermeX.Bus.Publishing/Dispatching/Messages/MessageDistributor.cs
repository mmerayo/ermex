using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
    /// <summary>
    /// Distributes the messages with an entry per subscriber where the message havent been sent
    /// </summary>
    sealed class MessageDistributor:ProducerParallelConsumerQueue<MessageDistributor.MessageDistributorMessage>
    {

        public class MessageDistributorMessage{
            public OutgoingMessage OutGoingMessage { get; private set; }
            public BusMessage Message { get; private set; }

            public MessageDistributorMessage(OutgoingMessage outGoingMessage, BusMessage message)
            {
                OutGoingMessage = outGoingMessage;
                Message = message;
                if (outGoingMessage == null) throw new ArgumentNullException("outGoingMessage");
                if (message == null) throw new ArgumentNullException("message");
                
            }
        }

        public MessageDistributor()
            : base(1, 16, 4, TimeSpan.FromSeconds(60))
        {
            
        }

        protected override Action<MessageDistributorMessage> RunActionOnDequeue
        {
            get { throw new NotImplementedException(); }
        }
    }
}
