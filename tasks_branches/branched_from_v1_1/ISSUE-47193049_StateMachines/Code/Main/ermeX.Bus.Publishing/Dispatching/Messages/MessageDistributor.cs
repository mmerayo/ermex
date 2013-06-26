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

using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.LayerMessages;
using ermeX.Models.Entities;
using ermeX.Parallel.Queues;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
	/// <summary>
	/// Distributes the messages creating an entry per subscriber where the message havent been sent
	/// </summary>
	internal sealed class MessageDistributor : ProducerParallelConsumerQueue<MessageDistributor.MessageDistributorMessage>,
	                                           IMessageDistributor
	{
		private readonly ICanReadOutgoingMessagesSubscriptions _outgoingMessagesSubscriptionsReader;
		private readonly IReadOutgoingQueue _outgoingQueueReader;
		private readonly IWriteOutgoingQueue _outgoingQueueWritter;
		private const int _maxThreadsNum = 128;
		private const int _queueSizeToCreateNewThread = 4;
		private const int _initialWorkerCount = 1;

		public class MessageDistributorMessage
		{
			public OutgoingMessage OutGoingMessage { get; private set; }

			public MessageDistributorMessage(OutgoingMessage outGoingMessage)
			{

				if (outGoingMessage == null) throw new ArgumentNullException("outGoingMessage");
				OutGoingMessage = outGoingMessage;
			}
		}

		[Inject]
		public MessageDistributor(ICanReadOutgoingMessagesSubscriptions outgoingMessagesSubscriptionsReader,
		                          IReadOutgoingQueue outgoingQueueReader,
		                          IWriteOutgoingQueue outgoingQueueWritter,
		                          IMessageSubscribersDispatcher dispatcher)
			: base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
		{
			Logger = LogManager.GetLogger<MessageDistributor>();
			_outgoingMessagesSubscriptionsReader = outgoingMessagesSubscriptionsReader;
			_outgoingQueueReader = outgoingQueueReader;
			_outgoingQueueWritter = outgoingQueueWritter;
			if (dispatcher == null) throw new ArgumentNullException("dispatcher");

			Dispatcher = dispatcher;
		}

		private IMessageSubscribersDispatcher Dispatcher { get; set; }


		protected override Func<MessageDistributorMessage, bool> RunActionOnDequeue
		{
			get { return OnDequeue; }
		}

		private readonly Dictionary<Guid, object> _subscriptorLockers = new Dictionary<Guid, object>();

		private bool OnDequeue(MessageDistributorMessage message)
		{
			if (message == null) throw new ArgumentNullException("message");
			bool result;
			try
			{
				//TODO: HIGHLY OPTIMIZABLE and THERe COULD BE CASES WHEN THE MESSAGE IS SENT TWICE
				OutgoingMessage outGoingMessage = message.OutGoingMessage;
				BusMessage busMessage = outGoingMessage.ToBusMessage();

				var subscriptions = GetSubscriptions(busMessage.Data.MessageType.FullName);

				foreach (var messageSuscription in subscriptions)
				{
					Guid destinationComponent = messageSuscription.Component;
					if (!_subscriptorLockers.ContainsKey(destinationComponent))
						lock (_subscriptorLockers)
							if (!_subscriptorLockers.ContainsKey(destinationComponent))
								_subscriptorLockers.Add(destinationComponent, new object());

					object subscriptorLocker = _subscriptorLockers[destinationComponent];
					lock (subscriptorLocker) // its sequential by component
					{
						//ensures it was not sent before this is not atomical because it will only happen when restarting or another component reconnecting
						if (_outgoingQueueReader.ContainsMessageFor(outGoingMessage.MessageId,
						                                            destinationComponent))
							continue;

						var messageToSend = outGoingMessage.GetClone(); //creates a copy for the subscriber
						messageToSend.Status = Message.MessageStatus.SenderDispatchPending; //ready to be dispatched
						messageToSend.PublishedTo = destinationComponent; //assigns the receiver
						
						Dispatcher.EnqueueItem(
							new MessageSubscribersDispatcher.SubscribersDispatcherMessage(messageToSend)); //pushes it

						Logger.TraceFormat("Ondequeue.Enqueued for dispatching message:{0}", messageToSend.MessageId);
						_outgoingQueueWritter.Save(messageToSend); //update the db
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("Thread:{1} There was an error while distributing an outgoing message. {0}", ex,Thread.CurrentThread.ManagedThreadId));
				result = false;
			}
			return result;
		}

		private IEnumerable<OutgoingMessageSuscription> GetSubscriptions(string typeFullName)
		{
			var result = new List<OutgoingMessageSuscription>();
			var types = TypesHelper.GetInheritanceChain(typeFullName, true);

			foreach (var type in types)
			{
				var outgoingMessageSuscriptions = _outgoingMessagesSubscriptionsReader.GetByMessageType(type.FullName);
				result.AddRange(outgoingMessageSuscriptions);
			}

			return result;
		}
	}
}