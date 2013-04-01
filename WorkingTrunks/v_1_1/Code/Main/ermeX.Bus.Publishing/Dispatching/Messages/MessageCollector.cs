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
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Observer;
using ermeX.Domain.Observers;
using ermeX.Domain.Queues;
using ermeX.Domain.Subscriptions;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
	/// <summary>
	/// Collects messages and restore the previous messages and remove expired messages
	/// </summary>
	internal sealed class MessageCollector : IMessagePublisherDispatcherStrategy,
	                                         IDomainObserver<OutgoingMessageSuscription>
	{
		private readonly IReadOutgoingQueue _outgoingReader;
		private readonly IWriteOutgoingQueue _outgoingWritter;
		private readonly IDomainObservable _domainNotifier;
		private const int CheckExpiredItemsWhenThisNumberOfMessagesWasDispatched = 100;
		private DispatcherStatus _status = DispatcherStatus.Stopped;

		[Inject]
		public MessageCollector(IBusSettings settings, IMessageDistributor distributor,
		                        IReadOutgoingQueue outgoingReader,
		                        IWriteOutgoingQueue outgoingWritter,
								IDomainObservable domainNotifier)
		{
			_outgoingReader = outgoingReader;
			_outgoingWritter = outgoingWritter;
			_domainNotifier = domainNotifier;
			
			Settings = settings;
			MessageDistributor = distributor;
		}

		#region IDisposable

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (Status == DispatcherStatus.Started)
						Stop();
				}

				_disposed = true;
			}
		}



		~MessageCollector()
		{
			Dispose(false);
		}

		#endregion

		private IBusSettings Settings { get; set; }
		private IMessageDistributor MessageDistributor { get; set; }
		private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
		private int _dispatchedItems = 0;

		#region IMessagePublisherDispatcherStrategy Members

		/// <summary>
		/// Dispatches one message
		/// </summary>
		/// <param name="message"></param>
		public void Dispatch(BusMessage message)
		{
			if (message == null) throw new ArgumentNullException("message");

			if (message.Data == null)
				throw new InvalidOperationException("the BusMessage cannot be null");

			if (Status != DispatcherStatus.Started)
				throw new InvalidOperationException("The component is not running now");

			Logger.Trace(x => x("{0} - Start dispatching", message.MessageId));

			OutgoingMessage outGoingMessage = CreateRootOutgoingMessage(message);
			MessageDistributor.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outGoingMessage));

			//every CheckExpiredItemsWhenThisNumberOfMessagesWasDispatched removes expired items
			if (++_dispatchedItems%CheckExpiredItemsWhenThisNumberOfMessagesWasDispatched == 0)
				SystemTaskQueue.Instance.EnqueueItem(RemoveExpiredMessages);

			Logger.Trace(x => x("MessageCollector: Dispatched message num: {0}", _dispatchedItems));
		}

		private void RemoveExpiredMessages()
		{
			TimeSpan expirationTime = Settings.SendExpiringTime;

			_outgoingWritter.RemoveExpiredMessages(expirationTime);
			Logger.Trace("MessageCollector: removed expired items");
		}

		private OutgoingMessage CreateRootOutgoingMessage(BusMessage message)
		{

			var result = new OutgoingMessage(message)
				{
					PublishedBy = message.Publisher,
					Status = Message.MessageStatus.SenderCollected

				};
			_outgoingWritter.Save(result);
			Logger.Trace("MessageCollector: Created RootOutgoingMessage");
			return result;
		}


		public DispatcherStatus Status
		{
			get { return _status; }
			private set
			{
				if (_status != value)
					_status = value;
			}
		}



		public void Start()
		{
			lock (this)
			{
				Status = DispatcherStatus.Starting;

				RemoveExpiredMessages();

				//subscribe to new subscriptions
				_domainNotifier.AddObserver(this);

				SendAllMessagesInQueue();

				Status = DispatcherStatus.Started;
			}
		}

		private void SendAllMessagesInQueue()
		{
			var outgoingMessages = _outgoingReader.GetByStatus(Message.MessageStatus.SenderCollected);
			foreach (var outgoingMessage in outgoingMessages)
				MessageDistributor.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outgoingMessage));
		}

		public void Notify(ObservableAction action, OutgoingMessageSuscription entity)
		{
			switch (action)
			{
				case ObservableAction.Add:
					SendMessagesPublishedBeforeSubscriptionWasReceived(entity);
					break;
				case ObservableAction.Update:
				case ObservableAction.Remove:

					break;
				default:
					throw new ArgumentOutOfRangeException("action");
			}
		}

		private void SendMessagesPublishedBeforeSubscriptionWasReceived(OutgoingMessageSuscription newSubscription)
		{
			var outgoingMessages = _outgoingReader.GetByStatus(Message.MessageStatus.SenderCollected);
			//TODO: Overload to satisfy this functionallity
			foreach (var outgoingMessage in outgoingMessages)
				if (outgoingMessage.CreatedTimeUtc >= newSubscription.DateLastUpdateUtc)
					//check if the subscription was generated before
					MessageDistributor.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outgoingMessage));
		}

		public void Stop()
		{
			lock (this)
			{
				Status = DispatcherStatus.Stopping;
				_domainNotifier.RemoveObserver(this);
				Status = DispatcherStatus.Stopped;
			}
		}

		#endregion
	}
}