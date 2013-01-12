// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ermeX.Threading.Queues;

namespace ermeX.Tests.Threading.Queues
{
    internal class TestProducerSequentialConsumerQueue : TestProducerConsumerQueueBase
    {

        protected class DummyQueue : ProducerSequentialConsumerQueue<DummyQueueItem>, ITestQueue
        {
            private readonly List<DummyQueueItem> _itemsRead = new List<DummyQueueItem>();

            private readonly object _locker = new object();


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
            return new DummyQueue();
        }

        [Test]
        public void Dont_Create_Threads_on_Demand()
        {
            bool increased = false;
            bool decreased = false;
            var dummyQueueItem = new DummyQueueItem(333, DateTime.Now);
            using(var testQueue = GetTarget())
            {
                const int numThreads = 5;
                const int numItemsToPush = 10;
                var threads = new Thread[numThreads];
                for(int i=0;i<numThreads;i++)
                    threads[i]=new Thread(()=>
                        {
                            for (int y = 0; y < numItemsToPush; y++)
                                testQueue.EnqueueItem(dummyQueueItem);
                        });
                for(int i=0;i<numThreads;i++)
                    threads[i].Start();
                int antThreads = 1;
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

                //tests removal
                var a = testQueue.CurrentThreadNumber;
                testQueue.EnqueueItem(dummyQueueItem);
                Thread.Sleep(250);
                Assert.AreEqual(1, a);

                Assert.AreEqual(1,maxThreads);
            }

            Assert.IsFalse(increased);
            Assert.IsFalse(decreased);
        }

        
    }
}
