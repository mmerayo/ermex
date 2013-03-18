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

            private bool _needSort = false;

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
                        throw new InvalidOperationException("The queue is empty");//SHOULD THIS RETURN NULL?
                    if (_needSort)
                    {
                        Queue.Sort(Comparer); //This mechanism must be improved
                        _needSort = false;
                    }
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
                    _needSort = true;
                }
            }
        }
    }
}