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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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


using ermeX.Entities.Entities;
using ermeX.LayerMessages;

namespace ermeX.Bus.Publishing.Dispatching
{

    //TODO: THIS TO BE DONE BY A WORKER,AND REMOVE THREAD
    internal sealed class MessageDispatcher : IMessagePublisherDispatcherStrategy
    {
        private static readonly object PollingMessageLocker = new object();
        private volatile bool _finishPolling;
        private TimeSpan _pollingTime = new TimeSpan(0, 0, 1);
        private DispatcherStatus _status = DispatcherStatus.Stopped;
        private Thread _threadPolling;
        private List<ISendingMessageWorker> _workers;

        [Inject]
        public MessageDispatcher(IBusSettings settings,IBusMessageDataSource busMessageDataSource )
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            Settings = settings;
            BusMessageDataSource = busMessageDataSource;
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

        #endregion

        private IBusSettings Settings { get; set; }
        private IBusMessageDataSource BusMessageDataSource { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);

      

        [Inject]
        private IOutgoingMessagesDataSource OutgoingMessagesDs { get; set; }


        [Inject]
        private IOutgoingMessageSuscriptionsDataSource MsgSuscriptionsDs { get; set; }

        #region IMessagePublisherDispatcherStrategy Members

        public void Dispatch(BusMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            if (message.Data == null )
                throw new InvalidOperationException("the BusMessage cannot be null");

            Logger.Trace(x=>x("{0} - Start dispatching", message.MessageId));

            var suscriptions = GetSubscriptions(message.Data.MessageType.FullName);
            OutgoingMessage outGoingMessage = CreateRootOutgoingMessage(message);

            if (suscriptions.Count > 0)
            {
                CreateEntryPerSubscriber(outGoingMessage, suscriptions);
            }

            

        }

        private OutgoingMessage CreateRootOutgoingMessage(BusMessage message)
        {
            BusMessageData busMessage = BusMessageData.FromBusLayerMessage(Settings.ComponentId, message, BusMessageData.BusMessageStatus.SenderOrder);
            BusMessageDataSource.Save(busMessage);


            var result = new OutgoingMessage(busMessage)
                             {
                                 PublishedBy = message.Publisher
                             };
            return result;
        }

        private IList<OutgoingMessageSuscription> GetSubscriptions(string typeFullName)
        {
            var result=new List<OutgoingMessageSuscription>();
            var types = TypesHelper.GetInheritanceChain(typeFullName,true);

            foreach (var type in types)
            {
                result.AddRange(MsgSuscriptionsDs.GetByMessageType(type.FullName));
            }

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

                _workers = new List<ISendingMessageWorker>();
                _threadPolling = new Thread(DoGetMessageToDeliver);

                _threadPolling.Start();
                Status = DispatcherStatus.Started;
            }
        }

        public void Stop()
        {
            lock (this)
            {
                Status = DispatcherStatus.Stopping;
                _finishPolling = true;
                _threadPolling.Join(_pollingTime.Add(_pollingTime + new TimeSpan(0, 0, 5)));
                if (_workers.Count > 0)
                {
                    foreach (var worker in _workers)
                    {
                        worker.Exit();
                    }
                    WaitHandle.WaitAll(_workers.Select(x=>(WaitHandle)x.FinishedEvent).ToArray(), new TimeSpan(0, 0, 15));
                    _workers.Clear();
                }

                _workers = null;
                Status = DispatcherStatus.Stopped;
            }
        }

        #endregion

        private void CreateEntryPerSubscriber(OutgoingMessage message,
                                             IEnumerable<OutgoingMessageSuscription> suscriptions)
        {
            if (suscriptions == null) throw new ArgumentNullException("suscriptions");

            
            BusMessageData busMessageData = BusMessageDataSource.GetById(message.BusMessageId);

            foreach (var messageSuscription in suscriptions)
            {
                //store bus message
                BusMessageData currentBusMessage = BusMessageData.NewFromExisting(busMessageData);
                currentBusMessage.Status=BusMessageData.BusMessageStatus.SenderDispatchPending;
                BusMessageDataSource.Save(currentBusMessage);

                //save outgoing message
                var messageToSend = message.GetClone();
                messageToSend.PublishedTo = messageSuscription.Component;
                messageToSend.BusMessageId = currentBusMessage.Id;
                OutgoingMessagesDs.Save(messageToSend);
                Logger.Trace(x=>x("{0} - Dispatching - Created entry for subscriber: {1}",currentBusMessage.MessageId,  messageSuscription.Component));
            }
        }
        
        private void DoGetMessageToDeliver()
        {
            try
            {
                while (!_finishPolling)
                {
                    if (_workers.Count < 64) //TODO: lock access to this variable
                        lock (PollingMessageLocker)
                            //TODO: CLEAR OPTIMIZATION HERE: the logic of the moller must keep pulling messages while they have different destinations, this is, every queue must have a subqueue per destination, DO WHEN QUEUES LOGIC TASK
                        {
                            OutgoingMessage nextDeliverable = OutgoingMessagesDs.GetNextDeliverable();
                            if (nextDeliverable != null)
                            {
                                nextDeliverable.Delivering = true;
                                OutgoingMessagesDs.Save(nextDeliverable);
                                AddToWorkingThread(nextDeliverable);
                            }
                        }

                    Thread.Sleep(_pollingTime);
                }
            }catch(Exception ex)
            {
                Logger.Warn(x=>x("DoGetMessageToDeliver Failed. {0}",ex));
            }
        }


        //TODO: TEST CAN RETRY SENDING MESSAGE WHEN FAILED

        private void AddToWorkingThread(OutgoingMessage outgoingMessage)
        {
            if (Status == DispatcherStatus.Started)
            {
                var handler = GetSendingMessageWorker(outgoingMessage);
                if (handler == null|| _workers==null) 
                    return;
                handler.PendingWorkFinished += HandlerFinished;
                _workers.Add(handler);

                handler.StartWorking(outgoingMessage);
            }
        }

        private void HandlerFinished(object sender, EventArgs eventArgs)
        {
            if (Status != DispatcherStatus.Stopping && Status != DispatcherStatus.Stopped)
            {
                var sendingMessageWorker = (ISendingMessageWorker) sender;
                sendingMessageWorker.Exit();
                sendingMessageWorker.FinishedEvent.WaitOne(TimeSpan.FromSeconds(10));
                _workers.Remove(sendingMessageWorker);
            }
        }


        protected internal ISendingMessageWorker GetSendingMessageWorker(OutgoingMessage outgoingMessage)
        {
            var sendingMessageWorker = IoCManager.Kernel.Get<ISendingMessageWorker>();//TODO: OPTIMIZE THIS
            return sendingMessageWorker;
        }

       
    }
}