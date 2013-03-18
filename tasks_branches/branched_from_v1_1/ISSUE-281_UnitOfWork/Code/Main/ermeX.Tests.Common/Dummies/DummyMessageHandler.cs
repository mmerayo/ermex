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
using System.Threading;
using ermeX.Bus.Listening.Handlers;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;

namespace ermeX.Tests.Common.Dummies
{

    internal class DummyMessageHandler : MessageHandlerBase<DummyDomainEntity>
    {
        public static Guid OperationId = Guid.NewGuid();
        private readonly Dictionary<Guid, AutoResetEvent> _eventHandled = new Dictionary<Guid, AutoResetEvent>();

        private readonly Dictionary<Guid, DummyDomainEntity> _receivedMessages =
            new Dictionary<Guid, DummyDomainEntity>();

        public DummyMessageHandler(bool asInternalMessageHandler = true)
        {
            if (asInternalMessageHandler)
                OperationId = typeof(ReceptionMessageHandler).GUID;
        }


        public Dictionary<Guid, DummyDomainEntity> ReceivedMessages
        {
            get { return _receivedMessages; }
        }

        public override object Handle(DummyDomainEntity message)
        {
            _receivedMessages.Add(message.Id, message);
            if (_eventHandled.ContainsKey(message.Id))
                _eventHandled[message.Id].Set();
            return null;
        }

        public override void Dispose()
        {
        }

        public void AddEvent(Guid id, AutoResetEvent eventHandled)
        {
            _eventHandled.Add(id, eventHandled);
        }
    }
}