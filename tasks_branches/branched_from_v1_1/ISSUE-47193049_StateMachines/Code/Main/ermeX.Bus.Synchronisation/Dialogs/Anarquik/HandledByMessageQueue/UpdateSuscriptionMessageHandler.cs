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

using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Logging;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByMessageQueue
{
    internal sealed class UpdateSuscriptionMessageHandler : IUpdateSuscriptionMessageHandler
    {
        private readonly ILogger Logger ;
        
        [Inject]
        public UpdateSuscriptionMessageHandler(IMessagePublisher publisher, 
			IMessageListener listener,IComponentSettings settings)
        {
	        Logger = LogManager.GetLogger(typeof (UpdateSuscriptionMessageHandler), settings.ComponentId,
	                                      LogComponent.Handshake);
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            Publisher = publisher;
            Listener = listener;
           
        }

        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }

        #region IUpdateSuscriptionMessageHandler Members

        public void HandleMessage(UpdateSuscriptionMessage message)
        {
            Logger.Debug(x => x("HandleMessage"));
            throw new NotImplementedException();
        }

        public void Start()
        {
            Logger.Debug(x=>x("Start"));
            Type handlerInterfaceType = GetType().GetInterface(typeof (IHandleMessages<>).FullName);
            Listener.Suscribe(handlerInterfaceType, this);
        }

        //public bool Evaluate(UpdateSuscriptionMessage message)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}