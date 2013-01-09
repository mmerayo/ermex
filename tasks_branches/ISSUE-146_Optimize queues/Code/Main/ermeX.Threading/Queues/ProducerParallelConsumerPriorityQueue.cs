using System;
using System.Collections.Generic;
using System.Linq;

namespace ermeX.Threading.Queues
{
    internal abstract class ProducerParallelConsumerPriorityQueue<TQueueItem> : ProducerParallelConsumerQueue<TQueueItem>
    {
        protected ProducerParallelConsumerPriorityQueue(int initialWorkerCount, int maxThreadsNum, IComparer<TQueueItem> comparer)
            : this(initialWorkerCount, maxThreadsNum, -1, TimeSpan.MaxValue,comparer)
        {
        }

        protected ProducerParallelConsumerPriorityQueue(int initialWorkerCount, int maxThreadsNum, int queueSizeToCreateNewThread, TimeSpan maxLazyThreadAlive,IComparer<TQueueItem> comparer) : base(initialWorkerCount, maxThreadsNum, queueSizeToCreateNewThread, maxLazyThreadAlive)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            ItemsQueue = new PriorityQueueWrapper< TQueueItem>(comparer);
        }

        private class PriorityQueueWrapper< TQueueItem> : IQueueWrapper<TQueueItem>
        {
            private readonly object _locker = new object();
            private List<TQueueItem> Queue { get; set; }
            private IComparer<TQueueItem> Comparer { get; set; }

            public PriorityQueueWrapper( IComparer<TQueueItem> comparer)
            {
                if (comparer == null) throw new ArgumentNullException("comparer");
                Queue = new List<TQueueItem>();
                Comparer = comparer;
            }

            public int Count
            {
                get { return Queue.Count; }
            }

            public TQueueItem Dequeue()
            {
                lock (_locker)
                {
                    if (Queue.Count == 0)
                        throw new InvalidOperationException("The queue is empty");
                    TQueueItem result = Queue[0];
                    Queue.RemoveAt(0);
                    return result;
                }
            }

            public void Enqueue(TQueueItem item)
            {
                lock (_locker)
                {
                    Queue.Add(item);
                    Queue.Sort(Comparer);
                }
            }
        }
    }
}