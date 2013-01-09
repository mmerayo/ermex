using System;
using System.Collections.Generic;
using System.Linq;

namespace ermeX.Threading.Queues
{
    internal abstract class ProducerParallelConsumerPriorityQueue<TKey,TQueueItem> : ProducerParallelConsumerQueue<TQueueItem>
    {
        protected ProducerParallelConsumerPriorityQueue(int initialWorkerCount, int maxThreadsNum, Func<TQueueItem, TKey> getSortByPriorityKey)
            : this(initialWorkerCount, maxThreadsNum, -1, TimeSpan.MaxValue,getSortByPriorityKey)
        {
        }

        protected ProducerParallelConsumerPriorityQueue(int initialWorkerCount, int maxThreadsNum, int queueSizeToCreateNewThread, TimeSpan maxLazyThreadAlive,Func<TQueueItem,TKey> getSortByPriorityKey ) : base(initialWorkerCount, maxThreadsNum, queueSizeToCreateNewThread, maxLazyThreadAlive)
        {
            if (getSortByPriorityKey == null) throw new ArgumentNullException("getSortByPriorityKey");
            ItemsQueue = new PriorityQueueWrapper<TKey, TQueueItem>(new SortedList<TKey, TQueueItem>(),
                                                                    getSortByPriorityKey);
        }

        private class PriorityQueueWrapper<TKey, TQueueItem> : IQueueWrapper<TQueueItem>
        {
            private readonly object _locker = new object();
            private SortedList<TKey, TQueueItem> Queue { get; set; }

            public PriorityQueueWrapper(SortedList<TKey, TQueueItem> queue, Func<TQueueItem, TKey> getKeyAction)
            {
                if (queue == null) throw new ArgumentNullException("queue");
                if (getKeyAction == null) throw new ArgumentNullException("getKeyAction");
                GetKey = getKeyAction;
                Queue = queue;
            }

            public int Count
            {
                get { return Queue.Count; }
            }

            private Func<TQueueItem, TKey> GetKey { get; set; }

            public TQueueItem Dequeue()
            {
                lock (_locker)
                {
                    if (Queue.Count == 0)
                        throw new InvalidOperationException("The queue is empty");
                    TQueueItem result = Queue.First().Value;
                    Queue.RemoveAt(0);
                    return result;
                }
            }

            public void Enqueue(TQueueItem item)
            {
                if (Equals(item, default(TQueueItem)))
                    throw new ArgumentNullException();
                lock (_locker)
                {
                    Queue.Add(GetKey(item), item);
                }
            }
        }
    }
}