using System;
using System.Collections.Generic;

namespace ermeX.Threading.Queues
{
    internal abstract class ProducerSequentialConsumerPriorityQueue<TQueueItem> : ProducerParallelConsumerPriorityQueue<TQueueItem>
    {
        protected ProducerSequentialConsumerPriorityQueue(IComparer<TQueueItem> getSortByPriorityKey)
            : base(1, 1, getSortByPriorityKey)
        {
        }
        
    }
}