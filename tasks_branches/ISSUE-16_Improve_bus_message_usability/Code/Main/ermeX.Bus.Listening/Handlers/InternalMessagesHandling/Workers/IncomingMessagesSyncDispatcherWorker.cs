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
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ninject;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers;
using ermeX.Common;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers
{
    //TODO: THIS SHOULD BE DONE BY THE BIZ LAYER OR TO BE PASSED TO THE bIZ LAYER, WHERE DELIVERY TO USER AND OR INVOCATES HANDLERS
    internal class IncomingMessagesSyncDispatcherWorker :Worker, IIncomingMessagesDispatcherWorker
    {

        [Inject]
        public IncomingMessagesSyncDispatcherWorker(IIncomingMessagesDataSource messagesDataSource,
            IScheduler scheduler)
            : base("IncomingMessagesSyncDispatcherWorker", new TimeSpan(0, 0, 1))
        //TODO, THIS VALUE TO BE CONFIGURABLE AND CHANGE FOR TESTING
        {
            if (messagesDataSource == null) throw new ArgumentNullException("messagesDataSource");
            if (scheduler == null) throw new ArgumentNullException("scheduler");
            MessagesDataSource = messagesDataSource;
            Scheduler = scheduler;
        }

        private IIncomingMessagesDataSource MessagesDataSource { get; set; }
        private IScheduler Scheduler { get; set; }

        #region IIncomingMessagesDispatcherWorker Members

        protected override void DoWork(object data)
        {
            var item = Scheduler.GetNext();
            if (item != null)
            {
                var busMessage = item.BusMessage;

                Logger.Trace(x=>x("{0} Start Handling", busMessage.MessageId));
                try
                {
                    OnDispatchMessage(item.SuscriptionHandlerId, busMessage);
                }
                catch
                {
                    item.Status=BusMessageData.BusMessageStatus.ReceiverDispatchable;
                    MessagesDataSource.Save(item);
                    throw;
                }
                
                MessagesDataSource.Remove(item);
                Logger.Trace(x=>x("{0} Handled finally",busMessage.MessageId));
            }
        }

        public event Action<Guid, object> DispatchMessage;

        #endregion

        //TODO: invocation must be done on the biz layer
        private void OnDispatchMessage(Guid suscriptionHandlerId, BusMessage message)
        {
            try
            {
                Action<Guid, object> handler = DispatchMessage;
                if (handler != null)
                {
                    handler(suscriptionHandlerId, message.Data.RawData);
                    Logger.Trace(x=>x("Receiver: Dispatched message with id: {0}", message.MessageId));
                }
                else
                {
                    Logger.Trace(x=>x("Receiver: The message with id: {0} didnt have receivers configured", message.MessageId));
                }

            }catch(Exception ex)
            {
                Logger.Error(x=>x("Error handling message {0} by subscriptionHandler {1}", message.MessageId, suscriptionHandlerId),ex);
                throw ex;
            }
        }

        
    }
}