using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ermeX.Threading.Queues;

namespace ermeX.Tests.Threading.Queues
{
    internal class TestProducerParallelConsumerQueue : TestProducerConsumerQueueBase
    {
        private const int InitialWorkerCount = 1;

        internal class DummyQueue : ProducerParallelConsumerQueue<DummyQueueItem>, ITestQueue
        {
            private readonly List<DummyQueueItem> _itemsRead = new List<DummyQueueItem>();

            private readonly object _locker = new object();

            public DummyQueue(int initialWorkerCount, int maxThreadsNum)
                : base(initialWorkerCount, maxThreadsNum)
            {
                
            }

            public DummyQueue(int initialWorkerCount, int maxThreadsNum, int queueSizeToCreateNewThread,
                              TimeSpan maxLazyThreadAlive)
                : base(initialWorkerCount, maxThreadsNum, queueSizeToCreateNewThread, maxLazyThreadAlive)
            {
            }

            protected override Action<DummyQueueItem> RunActionOnDequeue
            {
                get
                {
                    return (item) =>
                        {
                            lock (_locker)
                            {
                                ItemsRead.Add(item);
                            }
                        };
                }
            }

            public List<DummyQueueItem> ItemsRead
            {
                get { return _itemsRead; }
            }
        }

        protected override ITestQueue GetTarget()
        {
            return new DummyQueue(InitialWorkerCount, 64, 5, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void Create_Threads_And_Remove_Them_On_Demand()
        {
            bool increased = false;
            bool decreased = false;
            var dummyQueueItem = new DummyQueueItem(333, DateTime.Now);
            using(var testQueue = GetTarget())
            {
                const int numThreads = 32;
                const int numItemsToPush = 100;
                var threads = new Thread[numThreads];
                for(int i=0;i<numThreads;i++)
                    threads[i]=new Thread(()=>
                        {
                            for (int y = 0; y < numItemsToPush; y++)
                                testQueue.EnqueueItem(dummyQueueItem);
                        });
                for(int i=0;i<numThreads;i++)
                    threads[i].Start();
                int antThreads = InitialWorkerCount;
                int maxThreads = 0;
                int count;
                do
                {
                    var currentThreadNumber = testQueue.CurrentThreadNumber;
                    if (!increased && currentThreadNumber > antThreads)
                    {
                        increased = true;
                    }
                    else if (!decreased && currentThreadNumber < antThreads)
                    {
                        decreased = true;
                    }
                    antThreads = currentThreadNumber;
                    if (maxThreads < currentThreadNumber) 
                        maxThreads = currentThreadNumber;
                    count = testQueue.ItemsRead.Count;
                } while (count < numThreads * numItemsToPush);
                Thread.Sleep(TimeSpan.FromSeconds(5)); //make them expire
                
                Assert.IsTrue(maxThreads>2 && maxThreads<=64 );
            }

            Assert.IsTrue(increased);
            Assert.IsTrue(decreased);

        }
    }
}
