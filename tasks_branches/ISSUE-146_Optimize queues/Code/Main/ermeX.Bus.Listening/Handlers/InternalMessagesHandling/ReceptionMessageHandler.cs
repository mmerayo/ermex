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
using System.IO;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;
using ermeX.Threading.Scheduling;
using ermeX.Transport.Interfaces;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling
{
    /// <summary>
    /// Receives a transport message and initiates the incommingworkflow
    /// </summary>
    internal sealed class ReceptionMessageHandler : MessageHandlerBase<TransportMessage>
    {
        public static Guid OperationIdentifier = OperationIdentifiers.InternalMessagesOperationIdentifier;

        #region IDisposable

        //TODO: REMOVE IF NOT NEEDED
        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                   
                }
                _disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ReceptionMessageHandler()
        {
            Dispose(false);
        }
       

        #endregion


        [Inject]
        public ReceptionMessageHandler(
            IIncomingMessagesDataSource incomingMessagesDataSource, IReceptionMessageDistributor receptionMessageDistributor,
                                      IBusSettings settings, SystemTaskQueue systemTaskQueue,IQueueDispatcherManager queueDispatcherManager)
        {
            if (incomingMessagesDataSource == null) throw new ArgumentNullException("incomingMessagesDataSource");
            if (receptionMessageDistributor == null) throw new ArgumentNullException("receptionMessageDistributor");
            if (settings == null) throw new ArgumentNullException("settings");
            if (systemTaskQueue == null) throw new ArgumentNullException("systemTaskQueue");
            if (queueDispatcherManager == null) throw new ArgumentNullException("queueDispatcherManager");
            IncomingMessagesDataSource = incomingMessagesDataSource;
            ReceptionMessageDistributor = receptionMessageDistributor;
            Settings = settings;
            SystemTaskQueue = systemTaskQueue;
            QueueDispatcherManager = queueDispatcherManager;

            SystemTaskQueue.EnqueueItem(EnqueueNonDispatchedMessages);  //reenqueues non dispatched messages on startup
        }

       

        private IIncomingMessagesDataSource IncomingMessagesDataSource { get; set; }
        private IReceptionMessageDistributor ReceptionMessageDistributor { get; set; }
        private IBusSettings Settings { get; set; }
        private SystemTaskQueue SystemTaskQueue { get; set; }
        private IQueueDispatcherManager QueueDispatcherManager { get; set; }
        private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);


        public override object Handle(TransportMessage message)
        {
            BusMessage busMessage = message.Data;

            var incomingMessage = new IncomingMessage(BusMessage.Clone(busMessage))
            {
                ComponentOwner = Settings.ComponentId,

                PublishedTo = Settings.ComponentId,
                TimeReceivedUtc = DateTime.UtcNow,
                SuscriptionHandlerId = Guid.Empty,
                Status = Message.MessageStatus.ReceiverReceived,
            };
            
            //this must be done on-line in case of errors
            IncomingMessagesDataSource.Save(incomingMessage); 
            ReceptionMessageDistributor.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage));
            Logger.Trace(x=>x("{0} - Message received ", message.Data.MessageId));
            return null;
        }

        private void EnqueueNonDispatchedMessages()
        {
            //Gets all that werent delivered in previous sessions
            var incomingMessages = IncomingMessagesDataSource.GetByStatus(Message.MessageStatus.ReceiverReceived, Message.MessageStatus.ReceiverDispatchable, Message.MessageStatus.ReceiverDispatching);

            foreach (var incomingMessage in incomingMessages)
                ReceptionMessageDistributor.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage));
        }

        public void RegisterSuscriber(Action<Guid, object> onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException("onMessageReceived");
            Logger.Trace("InternalMessageHandler.RegisterSuscriber");

            QueueDispatcherManager.DispatchMessage += onMessageReceived;
        }
    }
}