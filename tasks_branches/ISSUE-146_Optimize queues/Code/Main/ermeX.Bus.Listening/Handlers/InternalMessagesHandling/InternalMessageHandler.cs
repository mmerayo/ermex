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
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling
{
    internal sealed class InternalMessageHandler : MessageHandlerBase<TransportMessage>
    {
        public static Guid OperationIdentifier = OperationIdentifiers.InternalMessagesOperationIdentifier;
        public static string IncomingFileExtension = "if";

        #region IDisposable

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DispatcherWorker.Exit();
                    ProcessorWorker.Exit();

                    WaitHandle.WaitAll(new WaitHandle[] {DispatcherWorker.FinishedEvent, ProcessorWorker.FinishedEvent});

                    //TODO: LOG THREADS DISPOSED
                }

                _disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

       

        #endregion


        [Inject]
        public InternalMessageHandler(IIncomingMessagesProcessorWorker processorWorker,
                                      IIncomingMessagesDispatcherWorker dispatcherWorker,
            IBusMessageDataSource busMessageDataSource,
                                      IBusSettings settings)
        {
            if (processorWorker == null) throw new ArgumentNullException("processorWorker");
            if (dispatcherWorker == null) throw new ArgumentNullException("dispatcherWorker");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            if (settings == null) throw new ArgumentNullException("settings");
            ProcessorWorker = processorWorker;
            DispatcherWorker = dispatcherWorker;
            BusMessageDataSource = busMessageDataSource;
            Settings = settings;
            
        }

        private IIncomingMessagesProcessorWorker ProcessorWorker { get; set; }
        private IIncomingMessagesDispatcherWorker DispatcherWorker { get; set; }
        private IBusMessageDataSource BusMessageDataSource { get; set; }
        private IBusSettings Settings { get; set; }
        private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);

     
        public void StartWorkers()
        {
            DispatcherWorker.StartWorking(null);
            ProcessorWorker.StartWorking(null);
        }

        public override object Handle(TransportMessage message)
        {
            BusMessage busMessage = message.Data;
            BusMessageDataSource.Save(BusMessageData.FromBusLayerMessage(Settings.ComponentId, busMessage,BusMessageData.BusMessageStatus.ReceiverReceived));
            
            Logger.Trace(x=>x("{0} - Message received ", message.Data.MessageId));
            ProcessorWorker.WorkPendingEvent.Set();
            return null;
        }

       

        public void RegisterSuscriber(Action<Guid, object> onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException("onMessageReceived");
            Logger.Trace("InternalMessageHandler.RegisterSuscriber");

            DispatcherWorker.DispatchMessage += onMessageReceived;
        }
    }
}