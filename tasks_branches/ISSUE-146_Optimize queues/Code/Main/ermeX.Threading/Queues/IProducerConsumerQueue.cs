using System;

namespace ermeX.Threading.Queues
{
    internal interface IProducerConsumerQueue<TQueueItem> : IDisposable
    {
        /// <summary>
        /// Number of threads active currently
        /// </summary>
        int CurrentThreadNumber { get; }

        int Count { get; }
        void EnqueueItem(TQueueItem item);
    }
}