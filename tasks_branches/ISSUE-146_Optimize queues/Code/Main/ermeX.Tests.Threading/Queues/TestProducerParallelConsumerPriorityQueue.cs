using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ermeX.Threading.Queues;

namespace ermeX.Tests.Threading.Queues
{
    internal class TestProducerParallelConsumerPriorityQueue : TestProducerConsumerQueueBase
    {
        private const int InitialWorkerCount = 1;

        protected class DummyQueue : ProducerParallelConsumerPriorityQueue<DummyQueueItem>, ITestQueue
        {
            private readonly List<DummyQueueItem> _itemsRead = new List<DummyQueueItem>();


            public DummyQueue(int initialWorkerCount, int maxThreadsNum)
                : base(initialWorkerCount, maxThreadsNum,new MyComparer())
            {
                
            }
            
            public DummyQueue(int initialWorkerCount, int maxThreadsNum, int queueSizeToCreateNewThread,
                              TimeSpan maxLazyThreadAlive)
                : base(initialWorkerCount, maxThreadsNum, queueSizeToCreateNewThread, maxLazyThreadAlive,new MyComparer())
            {
            }

            private class MyComparer : IComparer<DummyQueueItem>
           {
                public int Compare(DummyQueueItem x, DummyQueueItem y)
                {
                    if (x == null && y == null)
                        return 0;
                    if (x == null)
                        return -1;
                    if (y == null)
                        return 1;
                    
                    return x.Time.CompareTo(y.Time);
                }
           }

            protected override Action<DummyQueueItem> RunActionOnDequeue
            {
                get
                {
                    return (item) => ItemsRead.Add(item);
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
        public void SortsByPriority()
        {
            var queueItem = new DummyQueueItem(666, DateTime.Now.Subtract(TimeSpan.FromDays(1)));

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
                        //this will pass many of them
                        testQueue.EnqueueItem( queueItem);
                    }
                    else if (!decreased && currentThreadNumber < antThreads)
                    {
                        decreased = true;
                    }
                    antThreads = currentThreadNumber;
                    if (maxThreads < currentThreadNumber) 
                        maxThreads = currentThreadNumber;
                    count = testQueue.ItemsRead.Count;
                } while (count < numThreads * numItemsToPush +1 );
                Thread.Sleep(TimeSpan.FromSeconds(5)); //make them expire

                //tests removal
                var a = testQueue.CurrentThreadNumber;
                testQueue.EnqueueItem(dummyQueueItem);
                Thread.Sleep(250);
                Assert.IsTrue(a== testQueue.CurrentThreadNumber+1);

                Assert.IsTrue(maxThreads>2 && maxThreads<=64 );

                int indexOf = testQueue.ItemsRead.IndexOf(queueItem);
                int count1 = testQueue.ItemsRead.Count;

                Console.WriteLine("Index:{0} of {1}",indexOf,count1);
                Assert.IsTrue(indexOf<count1-1);
            }

            Assert.IsTrue(increased);
            Assert.IsFalse(decreased);
        }

        
    }
}
