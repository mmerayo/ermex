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
using System.Diagnostics;
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Common;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Models.Entities;
using ermeX.LayerMessages;
using ermeX.Parallel.Queues;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    internal class ReceptionMessageDistributor: ProducerParallelConsumerQueue<ReceptionMessageDistributor.MessageDistributorMessage>, IReceptionMessageDistributor
    {
	    private readonly ICanReadIncommingMessagesSubscriptions _incommingSubscriptionsReader;
	    private readonly IReadIncommingQueue _queueReader;
	    private readonly IWriteIncommingQueue _queueWritter;
	    private const int _maxThreadsNum = 128;
        private const int _queueSizeToCreateNewThread = 4;
        private const int _initialWorkerCount = 1;
        public class MessageDistributorMessage
        {
            public IncomingMessage IncomingMessage { get; private set; }

            public MessageDistributorMessage(IncomingMessage message)
            {
                if (message == null) throw new ArgumentNullException("message");
                IncomingMessage = message;
            }
        }

        [Inject]
        public ReceptionMessageDistributor(ICanReadIncommingMessagesSubscriptions incommingSubscriptionsReader,
			IReadIncommingQueue queueReader,
			IWriteIncommingQueue queueWritter,
                                   IQueueDispatcherManager dispatcher)
            : base(_initialWorkerCount, _maxThreadsNum, _queueSizeToCreateNewThread, TimeSpan.FromSeconds(60))
        {
			Logger = LogManager.GetLogger<ReceptionMessageDistributor>();
	        _incommingSubscriptionsReader = incommingSubscriptionsReader;
	        _queueReader = queueReader;
	        _queueWritter = queueWritter;
	        if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            Dispatcher = dispatcher;

        }

       

        private IQueueDispatcherManager Dispatcher { get; set; }


        protected override Func<MessageDistributorMessage, bool> RunActionOnDequeue
        {
            get
            {
                return OnDequeue;
            }
        }

        private readonly Dictionary<Guid, object> _subscriptorLockers = new Dictionary<Guid, object>();

        

        private bool OnDequeue(MessageDistributorMessage message) 
        {
            if (message == null) throw new ArgumentNullException("message");
            bool result;
            try
            {
                
                var incomingMessage = message.IncomingMessage;
                Debug.Assert(incomingMessage.Status==Message.MessageStatus.ReceiverReceived);

                BusMessage busMessage = incomingMessage.ToBusMessage();

                var subscriptions = GetSubscriptions(busMessage.Data.MessageType.FullName);
                
                foreach (var messageSuscription in subscriptions)
                {
                    Guid destination = messageSuscription.SuscriptionHandlerId;
                        //TODO: ISSUE-244--> IT GOES TO THE QUEUE AND THE QUEUE KEEPS THE HANDLERS
                    if (!_subscriptorLockers.ContainsKey(destination))
                        lock (_subscriptorLockers)
                            if (!_subscriptorLockers.ContainsKey(destination))
                                _subscriptorLockers.Add(destination, new object());

                    object subscriptorLocker = _subscriptorLockers[destination];
                    lock (subscriptorLocker) // its sequential by component
                    {
                        //ensures it was not sent before this is not atomical because it will only happen when restarting or another component reconnecting
                        if (_queueReader.ContainsMessageFor(incomingMessage.MessageId, destination))
                            continue;

                        var messageToDeliver = incomingMessage.GetClone(); //creates a copy for the subscriber
						messageToDeliver.MessageId=incomingMessage.MessageId;
                        messageToDeliver.Status = Message.MessageStatus.ReceiverDispatchable; //ready to be dispatched
                        messageToDeliver.SuscriptionHandlerId = destination;

						Logger.TraceFormat("OnDequeue. component:{2} -distributing messageid:{0} to {1}", incomingMessage.MessageId, destination, message.IncomingMessage.PublishedTo);
                        _queueWritter.Save(messageToDeliver); //update the db ? could this be done async?
                        Dispatcher.EnqueueItem(new QueueDispatcherManager.QueueDispatcherManagerMessage(messageToDeliver, true));//pushes it
						Logger.TraceFormat("OnDequeue. component:{2} -distributed messageid:{0} to {1}", incomingMessage.MessageId, destination, message.IncomingMessage.PublishedTo);
                    }
                }

                _queueWritter.Remove(message.IncomingMessage); //removes the original message
                result = true;

            }catch(Exception exception)
            {
                Logger.Error(x=>x("an error happened in the reception message distributor.: {0}",exception));
                //reenqueues the message
                result = false;
            }
            return result;
        }

        private IEnumerable<IncomingMessageSuscription> GetSubscriptions(string typeFullName)
        {
            var result = new List<IncomingMessageSuscription>();
            var types = TypesHelper.GetInheritanceChain(typeFullName, true);

            foreach (var type in types)
            {
                result.AddRange(_incommingSubscriptionsReader.GetByMessageType(type.FullName));
            }

            return result;
        }

        private void EnqueueNonDeliveredMessages()
        {
            try
            {
                //gets all that have been distributed non dispatched from previous sessions
                var incomingMessages = _queueReader.GetByStatus(Message.MessageStatus.ReceiverDispatchable,
                                                                      Message.MessageStatus.ReceiverDispatching);

                foreach (var incomingMessage in incomingMessages)
                {
                    incomingMessage.Status = Message.MessageStatus.ReceiverDispatchable;
                }

                _queueWritter.Save(incomingMessages);
                foreach (var incomingMessage in incomingMessages) //TODO:CREATE OVERLOAD on EnqueueItem TO ACCEPT THIS BATCH
                {
                    Dispatcher.EnqueueItem(new QueueDispatcherManager.QueueDispatcherManagerMessage(incomingMessage,
                                                                                                    false));
                }
            }catch(Exception ex)
            {
                Logger.Error(x=>x("{0}",ex));
                throw;
            }
        }

		//TODO: ENSURE THE COMPONENT IS NOT WORKING WHILE NOT STARTED IN OTHER METHODS
	    public override void Start()
	    {
            base.Start();
			EnqueueNonDeliveredMessages();
	    }
    }
}