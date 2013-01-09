using System;

namespace ermeX.Threading.Queues
{
    internal abstract class ProducerSequentialConsumerPriorityQueue<TKey,TValue>:ProducerParallelConsumerPriorityQueue<TKey,TValue>
    {
        protected ProducerSequentialConsumerPriorityQueue(Func<TValue, TKey> getSortByPriorityKey) : base(1, 1, getSortByPriorityKey)
        {
        }
        
    }
}