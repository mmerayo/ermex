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
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Queues;
using ermeX.Models.Entities;
using ermeX.Exceptions;
using ermeX.LayerMessages;
using ermeX.Parallel.Queues;
using ermeX.Parallel.Scheduling;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
	/// <summary>
	/// Sends the messages and handle sending failures
	/// </summary>
	internal sealed class MessageSubscribersDispatcher :
		ProducerParallelConsumerQueue<MessageSubscribersDispatcher.SubscribersDispatcherMessage>,
		IMessageSubscribersDispatcher
	{
		private readonly IReadOutgoingQueue _outgoingQueueReader;
		private readonly IWriteOutgoingQueue _outgoingQueueWritter;

		private const int _maxThreadsNum = 64;
		private const int _queueSizeToCreateNewThread = 4;
		private const int _initialWorkerCount = 4;

		public class SubscribersDispatcherMessage
		{
			public OutgoingMessage OutGoingMessage { get; private set; }

			public SubscribersDispatcherMessage(OutgoingMessage outGoingMessage)
			{
				if (outGoingMessage == null) throw new ArgumentNullException("outGoingMessage");
				OutGoingMessage = outGoingMessage;
			}
		}
		
		[Inject]
		public MessageSubscribersDispatcher(IBusSettings settings,
		                                    IReadOutgoingQueue outgoingQueueReader,
		                                    IWriteOutgoingQueue outgoingQueueWritter,
		                                    IJobScheduler taskScheduler,
		                                    IServiceProxy service)
			: base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
		{
			Logger = LogManager.GetLogger<MessageSubscribersDispatcher>();
			_outgoingQueueReader = outgoingQueueReader;
			_outgoingQueueWritter = outgoingQueueWritter;
			if (settings == null) throw new ArgumentNullException("settings");
			if (taskScheduler == null) throw new ArgumentNullException("taskScheduler");
			if (service == null) throw new ArgumentNullException("service");
			Settings = settings;
			JobScheduler = taskScheduler;
			Service = service;
		}

		private IBusSettings Settings { get; set; }
		private IJobScheduler JobScheduler { get; set; }
		private IServiceProxy Service { get; set; }

		protected override Func<SubscribersDispatcherMessage, bool> RunActionOnDequeue
		{
			get { return OnDequeue; }
		}

		private bool OnDequeue(SubscribersDispatcherMessage obj)
		{
			bool result;
			try
			{
				var messageToSend = obj.OutGoingMessage;
				if (messageToSend.Status != Message.MessageStatus.SenderDispatchPending)
					throw new InvalidOperationException(string.Format(
						"The message status is: {0} and is expected: {1}", messageToSend.Status,
						Message.MessageStatus.SenderDispatchPending.ToString()));

				if (messageToSend.Expired(Settings.SendExpiringTime))
				{
					messageToSend.Status = Message.MessageStatus.SenderFailed;
					Logger.Warn(
						x =>
						x("FATAL! {0} not sent to {1} AND EXPIRED.It wont be retried", messageToSend.MessageId,
						  messageToSend.PublishedTo));
					SystemTaskQueue.Instance.EnqueueItem(() => _outgoingQueueWritter.Save(messageToSend));
				}
				else
				{

					if (SendData(messageToSend))
					{
						messageToSend.Status = Message.MessageStatus.SenderSent;
						_outgoingQueueWritter.Save(messageToSend);
						Logger.TraceFormat("Component:{2} - MessageId: {0} Sent to {1}", messageToSend.MessageId, messageToSend.PublishedTo,Settings.ComponentId);
					}
					else
					{
						messageToSend.Tries += 1;

						_outgoingQueueWritter.Save(messageToSend);
						SystemTaskQueue.Instance.EnqueueItem(() => ReEnqueueItem(messageToSend));
						Logger.Warn(
							x =>
							x("FAILED! {0} not sent to {1}. It will be retried", messageToSend.MessageId,
							  messageToSend.PublishedTo));
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("There was an error while dispatching an outgoing message. {0}", ex));
				result = false; //we dont know the reason the base class manages reenqueue
			}
			return result; //true as we dont want to retry and every case is managed form this class
		}

		private bool SendData(OutgoingMessage data)
		{
			BusMessage busMessage = data.ToBusMessage();
			var transportMessage = new TransportMessage(data.PublishedTo, busMessage);

			ServiceResult serviceResult;
			try
			{
				serviceResult = Service.Send(transportMessage);
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("Couldn't send message {0} to {1}.{2}", busMessage.MessageId, data.PublishedTo, ex));
				return false;
			}
			if (!serviceResult.Ok)
			{
				if (serviceResult.ServerMessages == null || serviceResult.ServerMessages.Count == 0)
					Logger.Error("The service didnt return an error message at least. It is expected.");
				else
				{
					string logData = serviceResult.ServerMessages.Aggregate("Server returned errors: ",
					                                                        (current, serverMessage) =>
					                                                        current + Environment.NewLine +
					                                                        serverMessage);

					Logger.Error(x => x("{0}", logData));
				}
			}

			return serviceResult.Ok;
		}

		private void ReEnqueueItem(OutgoingMessage messageToSend)
		{
			if (messageToSend == null) throw new ArgumentNullException("messageToSend");

			//wait 5*tries seconds until a max of 2 minutes and retry send message
			int suggestedRetryTime = messageToSend.Tries*5;
			int fireTime = suggestedRetryTime > 120 ? 120 : suggestedRetryTime;

			var job = Job.At(this, DateTime.UtcNow.AddSeconds(fireTime),
			                 () => EnqueueItem(new SubscribersDispatcherMessage(messageToSend)));

			JobScheduler.ScheduleJob(job);
		}


		private void RestoreMessagesFromPreviousSessions()
		{
			var outgoingMessages = _outgoingQueueReader.GetByStatus(Message.MessageStatus.SenderDispatchPending);

			foreach (var outgoingMessage in outgoingMessages)
			{
				ReEnqueueItem(outgoingMessage);
			}
		}

		protected override void Dispose(bool disposing)
		{
			JobScheduler.RemoveJobsByRequester(this);
			base.Dispose(disposing);
		}

		public override void Start()
		{
			//TODO: CHECK IS STARTED IN THE OTHER OPERATIONS			
            RestoreMessagesFromPreviousSessions();
            base.Start();

		}
	}
}