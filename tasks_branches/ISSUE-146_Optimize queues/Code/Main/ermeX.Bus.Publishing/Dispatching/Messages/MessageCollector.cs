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
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
    /// <summary>
    /// Collects messages and restore the previous messages and remove expired messages
    /// </summary>
    internal sealed class MessageCollector : IMessagePublisherDispatcherStrategy
    {
        private const int CheckExpiredItemsWhenThisNumberOfMessagesWasDispatched = 100;
        private DispatcherStatus _status = DispatcherStatus.Stopped;

        [Inject]
        public MessageCollector(IBusSettings settings,
                                IBusMessageDataSource busMessageDataSource, SystemTaskQueue systemTaskQueue,
                                IOutgoingMessagesDataSource outgoingMessagesDataSource)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            if (systemTaskQueue == null) throw new ArgumentNullException("systemTaskQueue");
            if (outgoingMessagesDataSource == null) throw new ArgumentNullException("outgoingMessagesDataSource");
            Settings = settings;
            BusMessageDataSource = busMessageDataSource;
            SystemTaskQueue = systemTaskQueue;
            OutgoingMessagesDataSource = outgoingMessagesDataSource;
            MessageDistributor = new MessageDistributor();

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
        private IBusMessageDataSource BusMessageDataSource { get; set; }
        private SystemTaskQueue SystemTaskQueue { get; set; }
        private IOutgoingMessagesDataSource OutgoingMessagesDataSource { get; set; }
        private MessageDistributor MessageDistributor { get; set; }
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

            Logger.Trace(x => x("{0} - Start dispatching", message.MessageId));

            OutgoingMessage outGoingMessage = CreateRootOutgoingMessage(message);
            MessageDistributor.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outGoingMessage, message));

            //every CheckExpiredItemsWhenThisNumberOfMessagesWasDispatched removes expired items
            if (++_dispatchedItems%CheckExpiredItemsWhenThisNumberOfMessagesWasDispatched == 0)
                SystemTaskQueue.EnqueueItem(RemoveExpiredMessages);

            Logger.Trace(x => x("MessageCollector: Dispatched message num: {0}", _dispatchedItems));
        }

        private void RemoveExpiredMessages()
        {
            TimeSpan expirationTime = Settings.SendExpiringTime;
            var outgoingMessages = OutgoingMessagesDataSource.GetExpiredMessages(expirationTime);
            OutgoingMessagesDataSource.RemoveExpiredMessages(expirationTime);
            foreach (var outgoingMessage in outgoingMessages)
            {
                BusMessageDataSource.RemoveById(outgoingMessage.Id);
            }
            Logger.Trace("MessageCollector: removed expired items");
        }

        private OutgoingMessage CreateRootOutgoingMessage(BusMessage message)
        {
            BusMessageData busMessage = BusMessageData.FromBusLayerMessage(Settings.ComponentId, message,
                                                                           BusMessageData.BusMessageStatus.SenderOrder);
            BusMessageDataSource.Save(busMessage);

            var result = new OutgoingMessage(busMessage)
                {
                    PublishedBy = message.Publisher
                };
            OutgoingMessagesDataSource.Save(result);
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

                //TODO: OUTGOING MESSAGE SHOULD HOLD BUSMESSAGEdATA
                var busMessageDatas = BusMessageDataSource.GetByIdStatus(BusMessageData.BusMessageStatus.SenderOrder);
                foreach (var busMessageData in busMessageDatas)
                {
                    var outgoingMessage = OutgoingMessagesDataSource.GetByBusMessageId(busMessageData.Id);
                    if(!outgoingMessage.Expired(Settings.SendExpiringTime))
                        MessageDistributor.EnqueueItem(new MessageDistributor.MessageDistributorMessage(outgoingMessage,
                                                                                                    busMessageData));
                }

                Status = DispatcherStatus.Started;
            }
        }

        public void Stop()
        {
            lock (this)
            {
                Status = DispatcherStatus.Stopping;
                Status = DispatcherStatus.Stopped;
            }
        }

        #endregion
    }
}