using System;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.Threading.Queues
{
    /// <summary>
    /// Several overlapped consumers can pick messages from the queue 
    /// </summary>
    /// <typeparam name="TQueueItem"></typeparam>
    internal abstract class ProducerParallelConsumerQueue<TQueueItem>: IProducerConsumerQueue<TQueueItem>
    {
        private int MaxThreadsNum { get; set; }
        private int QueueSizeToCreateNewThread { get; set; }
        private readonly object _queueLocker = new object();
        private readonly object _threadsLocker = new object();
        private readonly List<Thread> _workers = new List<Thread>();
        
        private TimeSpan MaxLazyThreadAlive { get; set; }
        protected abstract Action<TQueueItem> RunActionOnDequeue { get; }
        protected readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);

        /// <summary>
        /// Number of threads active currently
        /// </summary>
        public int CurrentThreadNumber
        {
            get { return _workers.Count; }
        }

        protected ProducerParallelConsumerQueue(int initialWorkerCount, int maxThreadsNum):this(initialWorkerCount,maxThreadsNum,-1,TimeSpan.MaxValue)
        {
        }

        protected ProducerParallelConsumerQueue(int initialWorkerCount,int maxThreadsNum, int queueSizeToCreateNewThread,TimeSpan maxLazyThreadAlive)
        {
            if(initialWorkerCount<1)
                throw new ArgumentOutOfRangeException("initialWorkerCount","Must be at least 0");
            if(queueSizeToCreateNewThread==0)
                throw new ArithmeticException("The size to create a new thread cannot be 0");
            ItemsQueue = new QueueWrapper<TQueueItem>(new Queue<TQueueItem>());

            MaxThreadsNum = maxThreadsNum;
            QueueSizeToCreateNewThread = queueSizeToCreateNewThread;
            MaxLazyThreadAlive = maxLazyThreadAlive;
            for (int i = 0; i < initialWorkerCount; i++)
                AddNewThread();
        }
        protected IQueueWrapper<TQueueItem> ItemsQueue { get; set; }

        public int Count
        {
            get { return ItemsQueue.Count; }
            
        }

        private readonly Dictionary<string,DateTime> _timeLazy =new Dictionary<string, DateTime>();   
        private void AddNewThread()
        {
            if (_workers.Count < MaxThreadsNum)
                lock (_threadsLocker)
                    if (!_shuttingDown && _workers.Count < MaxThreadsNum)
                    {
                        var thread = new Thread(Consume){Name = string.Format("TC{0}", Guid.NewGuid().ToString())};
                        _timeLazy.Add(thread.Name,DateTime.UtcNow);
                        _workers.Add(thread);
                        thread.Start();
                    }
        }

        private bool RemoveThread(Thread thread)
        {
            bool result = false;
            if (_workers.Count > 1 || _shuttingDown)
                lock (_threadsLocker)
                    if (_workers.Count > 1 || _shuttingDown)
                    {
                        _timeLazy.Remove(thread.Name);
                        _workers.Remove(thread);
                        result = true;
                    }
            return result;
        }

        private bool _shuttingDown = false;
        private void Shutdown(bool waitForWorkers)
        {
            _shuttingDown = true;
            while(_workers.Count>0)
                EnqueueItem(default(TQueueItem));

            if (waitForWorkers)
                while( _workers.Count>0)
                    _workers[0].Join();
        }

        public void EnqueueItem(TQueueItem item)
        {
            lock (_queueLocker)
            {               
                ItemsQueue.Enqueue(item);
                Monitor.Pulse(_queueLocker);
                int count = ItemsQueue.Count;
                if(count > 0 && count % QueueSizeToCreateNewThread==0)
                    AddNewThread();
            }
        }

        private void Consume()
        {
            Thread currentThread = Thread.CurrentThread;

            while (true) 
            {
                TQueueItem item;
                bool mustExpireThread = false;
                lock (_queueLocker)
                {
                    _timeLazy[currentThread.Name] = DateTime.UtcNow;
                    while (ItemsQueue.Count == 0)
                    {
                        Monitor.Wait(_queueLocker);
                    }

                    mustExpireThread = DateTime.UtcNow.Subtract(_timeLazy[currentThread.Name]) >
                                       MaxLazyThreadAlive;
                    
                    item = ItemsQueue.Dequeue();
                    mustExpireThread = mustExpireThread || Equals(item, default(TQueueItem));
                }
                try
                {
                    if(item!=null)
                        RunActionOnDequeue(item);
                }catch(Exception ex)
                {
                    Logger.Error(x=>x("{0}",ex.ToString()));
                }
                finally
                {
                    if(mustExpireThread)
                        mustExpireThread=RemoveThread(currentThread);
                }
                if (mustExpireThread)
                    return;
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shutdown(disposing);
            }
        }

        ~ProducerParallelConsumerQueue()
        {
            Dispose(false);
        } 
        #endregion

        #region QueueWrapper

        protected interface IQueueWrapper<TQueueItem>
        {
            int Count { get; }
            TQueueItem Dequeue();
            void Enqueue(TQueueItem item);
        }

        private class QueueWrapper<TQueueItem> : IQueueWrapper<TQueueItem>
        {
            private Queue<TQueueItem> Queue { get; set; }

            public QueueWrapper(Queue<TQueueItem> queue )
            {
                if (queue == null) throw new ArgumentNullException("queue");
                Queue = queue;
            }

            public int Count { get { return Queue.Count; } }
            public TQueueItem Dequeue()
            {
                return Queue.Dequeue();
            }

            public void Enqueue(TQueueItem item)
            {
                Queue.Enqueue(item);
            }
        }

        

        #endregion


    }

    
}
