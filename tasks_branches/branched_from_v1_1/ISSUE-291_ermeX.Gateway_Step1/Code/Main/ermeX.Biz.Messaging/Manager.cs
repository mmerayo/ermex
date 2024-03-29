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
using Common.Logging;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.ConfigurationManagement.Settings;


using ermeX.LayerMessages;

namespace ermeX.Biz.Messaging
{
    internal class Manager : IMessagingManager
    {
        [Inject]
        public Manager(IBusSettings settings, IMessagePublisher publisher, IMessageListener listener)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            Settings = settings;
            Publisher = publisher;
            Listener = listener;
        }

        private IBusSettings Settings { get; set; }
        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);

        #region IMessagingManager Members

        public void PublishMessage(BizMessage message)
        {
            try
            {
                var busMessage = new BusMessage(Settings.ComponentId, message);
                Logger.Trace(x=>x("{0} - Created BusMessage", busMessage.MessageId));
                Publisher.PublishMessage(busMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "PublishMessage. {0}", ex));
                throw ex;
            }
        }

        #endregion

        
    }
}