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
using ermeX.Common;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByMessageQueue
{
    internal class UpdatePublishedServiceMessageHandler : IUpdatePublishedServiceMessageHandler
    {
        [Inject]
        public UpdatePublishedServiceMessageHandler(IMessagePublisher publisher, IMessageListener listener)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            Publisher = publisher;
            Listener = listener;
            Type handlerInterfaceType = GetType().GetInterface(typeof(IHandleMessages<>).FullName);
            Listener.Suscribe(handlerInterfaceType, this);
        }

        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }

        #region IUpdatePublishedServiceMessageHandler Members

        public void HandleMessage(UpdatePublishedServiceMessage message)
        {
            throw new NotImplementedException();
        }

        //public bool Evaluate(UpdatePublishedServiceMessage message)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}