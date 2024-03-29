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
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;

using ermeX.Entities.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
    internal sealed class MessageSuscriptionsRequestMessageHandler : IMessageSuscriptionsService
    {
        [Inject]
        public MessageSuscriptionsRequestMessageHandler(IMessagePublisher publisher,
                                                        IMessageListener listener, IComponentSettings settings,
                                                        IIncomingMessageSuscriptionsDataSource
                                                            incomingMessageSuscriptionsDataSource,
                                                        IOutgoingMessageSuscriptionsDataSource
                                                            outgoingMessageSuscriptionsDataSource, 
            IStatusManager statusManager)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (settings == null) throw new ArgumentNullException("settings");
            if (incomingMessageSuscriptionsDataSource == null)
                throw new ArgumentNullException("incomingMessageSuscriptionsDataSource");
            if (outgoingMessageSuscriptionsDataSource == null)
                throw new ArgumentNullException("outgoingMessageSuscriptionsDataSource");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            Publisher = publisher;
            Listener = listener;
            Settings = settings;
            IncomingMessageSuscriptionsDataSource = incomingMessageSuscriptionsDataSource;
            OutgoingMessageSuscriptionsDataSource = outgoingMessageSuscriptionsDataSource;
            StatusManager = statusManager;
        }

        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }
        private IComponentSettings Settings { get; set; }
        private IIncomingMessageSuscriptionsDataSource IncomingMessageSuscriptionsDataSource { get; set; }
        private IOutgoingMessageSuscriptionsDataSource OutgoingMessageSuscriptionsDataSource { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
        private IStatusManager StatusManager { get; set; }

        #region IMessageSuscriptionsService Members

        public MessageSuscriptionsResponseMessage RequestSuscriptions(MessageSuscriptionsRequestMessage request)
        {
            var result = new MessageSuscriptionsResponseMessage(Settings.ComponentId, request.CorrelationId)
                             {
                                 MyIncomingSuscriptions = IncomingMessageSuscriptionsDataSource.GetAll(),
                                 MyOutgoingSuscriptions =
                                     OutgoingMessageSuscriptionsDataSource.GetAll().Where(
                                         x => x.Component != request.SourceComponentId).ToList()
                             };

            return result;
        }

        public void AddSuscription(IncomingMessageSuscription request)
        {
            StatusManager.WaitIsRunning();
            try
            {
                if (request == null) throw new ArgumentNullException("request");
                OutgoingMessageSuscriptionsDataSource.SaveFromOtherComponent(request);
            }
            catch (Exception ex)
            {
                Logger.Warn(x=>x("Could not handle request", ex));
            }
        }



        public void AddSuscriptions(IList<IncomingMessageSuscription> request)
        {
            try
            {
                StatusManager.WaitIsRunning();
                foreach (var incomingMessageSuscription in request)
                {
                    AddSuscription(incomingMessageSuscription);
                }
            }
            catch(Exception ex)
            {
                Logger.Warn(x=>x("Could not handle request", ex));
            }
        }

        #endregion
    }
}