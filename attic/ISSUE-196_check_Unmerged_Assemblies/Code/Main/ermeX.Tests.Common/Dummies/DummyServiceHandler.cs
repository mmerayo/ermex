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
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyServiceHandler : ServiceHandlerBase
    {
        public static Guid OperationId = Guid.NewGuid();
        private readonly AutoResetEvent _eventHandled;

        public DummyServiceHandler(AutoResetEvent eventHandled)
        {
            _eventHandled = eventHandled;
        }

        public IDictionary<string, ServiceRequestMessage.RequestParameter> ReceivedMessage { get; private set; }


        public override object Handle(IDictionary<string, ServiceRequestMessage.RequestParameter> message)
        {
            ReceivedMessage = message;
            _eventHandled.Set();
            return null;
        }

        public override void Dispose()
        {
        }
    }
}