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

using ermeX.Logging;

namespace ermeX.Parallel.Queues
{
	/// <summary>
	/// Several overlapped consumers can pick messages from the queue using this one
	/// </summary>
	/// <typeparam name="TQueueItem"></typeparam>
	internal abstract class ProducerParallelConsumerQueue<TQueueItem> : IProducerConsumerQueue<TQueueItem>
	{
		private readonly int _initialWorkerCount;
		private int MaxThreadsNum { get; set; }
		private int QueueSizeToCreateNewThread { get; set; }
		private readonly object _queueLocker = new object();
		private readonly object _threadsLocker = new object();
		private readonly List<Thread> _workers = new List<Thread>();

		private TimeSpan MaxLazyThreadAlive { get; set; }

		/// <summary>
		/// handle the deliverty from the queue, when it returns false its reenqueued
		/// </summary>
		protected abstract Func<TQueueItem, bool> RunActionOnDequeue { get; }

		protected ILogger Logger
		{
			get { return _logger; }
			set { _logger = value; }
		}


		/// <summary>
		/// Number of threads active currently
		/// </summary>
		public int CurrentThreadNumber
		{
			get { return _workers.Count; }
		}

		protected ProducerParallelConsumerQueue(int initialWorkerCount, int maxThreadsNum)
			: this(initialWorkerCount, maxThreadsNum, -1, TimeSpan.MaxValue)
		{
		}

		protected ProducerParallelConsumerQueue(int initialWorkerCount, int maxThreadsNum, int queueSizeToCreateNewThread,
		                                        TimeSpan maxLazyThreadAlive)
		{
			_initialWorkerCount = initialWorkerCount;
			if (initialWorkerCount < 1)
				throw new ArgumentOutOfRangeException("initialWorkerCount", "Must be at least 0");
			if (queueSizeToCreateNewThread == 0)
				throw new ArithmeticException("The size to create a new thread cannot be 0");
			ItemsQueue = new QueueWrapper<TQueueItem>(new Queue<TQueueItem>());

			MaxThreadsNum = maxThreadsNum;
			QueueSizeToCreateNewThread = queueSizeToCreateNewThread;
			MaxLazyThreadAlive = maxLazyThreadAlive;

		}

		public virtual void Start()
		{
			//TODO: ALL THE METHODS TO ENSURE THAT THE COMPONENT IS RUNNING when invoked
			for (int i = 0; i < _initialWorkerCount; i++)
				AddNewThread();
		}

		public virtual void Stop()
		{
			Shutdown(true);
		}

		protected IQueueWrapper<TQueueItem> ItemsQueue { get; set; }

		public int Count
		{
			get { return ItemsQueue.Count; }

		}

		private readonly Dictionary<string, DateTime> _timeLazy = new Dictionary<string, DateTime>();

		private void AddNewThread()
		{
			if (_workers.Count < MaxThreadsNum)
				lock (_threadsLocker)
					if (!_shuttingDown && _workers.Count < MaxThreadsNum)
					{
						var thread = new Thread(Consume) {Name = string.Format("TC{0}", Guid.NewGuid().ToString())};
						_timeLazy.Add(thread.Name, DateTime.UtcNow);
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
		private static ILogger _logger = LogManager.GetNonQualifiedLogger<ProducerParallelConsumerQueue<TQueueItem>>();

		private void Shutdown(bool waitForWorkers)
		{
			_shuttingDown = true;
			while (_workers.Count > 0)
				EnqueueItem(default(TQueueItem));

			if (waitForWorkers)
				while (_workers.Count > 0)
					_workers[0].Join();
		}

		public void EnqueueItem(TQueueItem item)
		{
			lock (_queueLocker)
			{
				ItemsQueue.Enqueue(item);
				Monitor.Pulse(_queueLocker);
				int count = ItemsQueue.Count;
				if (count > 0 && count%QueueSizeToCreateNewThread == 0)
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
					if (!Equals(item, default(TQueueItem)) && !Disposed)
						if (!RunActionOnDequeue(item))
						{
							EnqueueItem(item);
						}
				}
				catch (Exception ex)
				{
					EnqueueItem(item);
					Logger.Error("An unhandled exception happened in the queue listener, the item has been reenqueued. ",ex);
				}
				finally
				{
					if (mustExpireThread)
						mustExpireThread = RemoveThread(currentThread);
				}
				if (mustExpireThread)
					return;
			}
		}

		#region IDisposable

		protected bool Disposed { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Shutdown(disposing);
			}

			Disposed = true;
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

			public QueueWrapper(Queue<TQueueItem> queue)
			{
				if (queue == null) throw new ArgumentNullException("queue");
				Queue = queue;
			}

			public int Count
			{
				get { return Queue.Count; }
			}

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