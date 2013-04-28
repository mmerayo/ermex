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
using System.Threading;
using NUnit.Framework;
using ermeX.Parallel.Queues;

namespace ermeX.Tests.Parallel.Queues
{
    internal sealed class TestProducerSequentialConsumerQueue : TestProducerConsumerQueueBase
    {
        private class DummyQueue : ProducerSequentialConsumerQueue<DummyQueueItem>, ITestQueue
        {
            public bool FailWhenHandling { get; set; }
            private int _numErrors = 0;
            private readonly List<DummyQueueItem> _itemsRead = new List<DummyQueueItem>();

            private readonly object _locker = new object();

            public DummyQueue(bool failWhenHandling=false)
            {
                FailWhenHandling = failWhenHandling;
            }


            protected override Func<DummyQueueItem, bool> RunActionOnDequeue
            {
                get
                {
                    if(FailWhenHandling)
                    {
                        throw new InvalidOperationException("Test Exception message. Errors: " + ++_numErrors);
                    }

                    return (item) =>
                        {
                            lock (_locker)
                            {
                                ItemsRead.Add(item);
                            }
                            return true;
                        };
                }
            }

            public List<DummyQueueItem> ItemsRead
            {
                get { return _itemsRead; }
            }

            public int NumErrors
            {
                get { return _numErrors; }
            }
        }

        protected override ITestQueue GetTarget(bool failWhenHandling = false)
        {
            var dummyQueue = new DummyQueue(failWhenHandling);
            dummyQueue.Start();
            return dummyQueue;
        }

        [Test]
        public void When_Handling_Fails_Item_Is_Pushed_Back_To_Queue()
        {
            var dummyQueueItem = new DummyQueueItem(333, DateTime.Now);
            using (var testQueue = (DummyQueue)GetTarget(true))
            {
                testQueue.EnqueueItem(dummyQueueItem);
                Thread.Sleep(TimeSpan.FromMilliseconds(250)); //TODO: THIS IS NOT DETERMINISTIC
                Console.WriteLine(testQueue.NumErrors);
                Assert.IsTrue(testQueue.NumErrors>1); //WE NEED IT TO FAIL AT LEAST TWICE SO that asserts that was repushed in the queue
            }


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
                var threads = new Thread[numThreads]; //create many threads to push on the queue
                for(int i=0;i<numThreads;i++)
                    threads[i]=new Thread(()=>
                        {
                            for (int y = 0; y < numItemsToPush; y++)
                                testQueue.EnqueueItem(dummyQueueItem);
                        });
                for(int i=0;i<numThreads;i++)  //start the threads
                    threads[i].Start();
                int antThreads = 1;
                int maxThreads = 0;
                int count;
                do
                {
                    var currentThreadNumber = testQueue.CurrentThreadNumber;  //detect and record the num of threads changes
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
