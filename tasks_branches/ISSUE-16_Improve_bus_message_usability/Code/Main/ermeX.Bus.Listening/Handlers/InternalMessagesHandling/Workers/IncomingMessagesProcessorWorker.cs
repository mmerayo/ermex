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
using System.Linq;
using System.Threading;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers
{
    internal class IncomingMessagesProcessorWorker : Worker, IIncomingMessagesProcessorWorker
    {

        [Inject]
        public IncomingMessagesProcessorWorker(IBusSettings settings,
                                               IIncomingMessageSuscriptionsDataSource suscriptionsDataSource,
                                               IIncomingMessagesDataSource messagesDatasource,
                                               IAppComponentDataSource componentDataSource,
            IIncomingMessagesDispatcherWorker dispatcherWorker)
            : base("IncomingMessagesProcessorWorker")
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (suscriptionsDataSource == null) throw new ArgumentNullException("suscriptionsDataSource");
            if (messagesDatasource == null) throw new ArgumentNullException("messagesDatasource");
            if (componentDataSource == null) throw new ArgumentNullException("componentDataSource");
            if (dispatcherWorker == null) throw new ArgumentNullException("dispatcherWorker");
            Settings = settings;
            SuscriptionsDataSource = suscriptionsDataSource;
            MessagesDatasource = messagesDatasource;
            ComponentDataSource = componentDataSource;
            DispatcherWorker = dispatcherWorker;
        }

        private IBusSettings Settings { get; set; }
        private IIncomingMessageSuscriptionsDataSource SuscriptionsDataSource { get; set; }
        private IIncomingMessagesDataSource MessagesDatasource { get; set; }
        private IAppComponentDataSource ComponentDataSource { get; set; }
        private IIncomingMessagesDispatcherWorker DispatcherWorker { get; set; }
        private readonly object _SyncLock=new object();
        #region IIncomingMessagesProcessorWorker Members

        protected override void DoWork(object data)
        {
            lock (_SyncLock)
            {
                var messagesToDispatch = MessagesDatasource.GetMessagesToDispatch();

                foreach (var message in messagesToDispatch)
                {
                    //deserialize

                    //Logger.Trace(x => x("{0} - Start Processing from {1}", message.MessageId, message.Publisher));

                    //update component Latency
                    //TODO: FROM TRANSPORT LAYER ?? check correctness as the meaning time is this
                    
                    UpdateComponentLatency(message.BusMessage, message.TimeReceivedUtc);

                    //get internal suscriptions
                    BizMessage bizMessage = message.BusMessage.Data;
                        
                    var suscriptions = GetSubscriptions(bizMessage.MessageType.FullName);

                    foreach (var suscription in suscriptions)
                    {
                        //create an object per suscription    
                        var incomingMessage = new IncomingMessage(BusMessage.Clone(message.BusMessage))
                            {
                                ComponentOwner = Settings.ComponentId,
                                PublishedTo = Settings.ComponentId,
                                TimeReceivedUtc = message.TimeReceivedUtc,
                                TimePublishedUtc = message.TimePublishedUtc,
                                PublishedBy = message.PublishedBy,
                                SuscriptionHandlerId = suscription.SuscriptionHandlerId,
                                Status=BusMessageData.BusMessageStatus.ReceiverDispatchable
                            };

                        //WRITE objects to DB in single transaction
                        MessagesDatasource.Save(incomingMessage);

                        Logger.Trace(
                            x =>
                            x("{0} - Created entry for handler {1}", message.BusMessage.MessageId, suscription.HandlerType));
                    }
                    //removes incommin created
                    MessagesDatasource.Remove(message); 
                    DispatcherWorker.WorkPendingEvent.Set();
                }
            }
        }

       

        #endregion

        private IEnumerable<IncomingMessageSuscription> GetSubscriptions(string typeFullName)
        {
            var result = new List<IncomingMessageSuscription>();
            var types = TypesHelper.GetInheritanceChain(typeFullName,true);

            foreach (var type in types)
            {
                result.AddRange(SuscriptionsDataSource.GetByMessageType(type.FullName));
            }

            return result;
        }

        private void UpdateComponentLatency(BusMessage receivedMessage, DateTime receivedTimeUtc)
        {
            var milliseconds = receivedTimeUtc.Subtract(receivedMessage.CreatedTimeUtc).Milliseconds;
            if (milliseconds <= (Settings.MaxDelayDueToLatencySeconds*1000))
            {
                ComponentDataSource.UpdateRemoteComponentLatency(receivedMessage.Publisher, milliseconds);
            }
        }

    }
}