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

namespace ermeX.Tests.WorldGateTests.Mock.Messages
{
    internal abstract class TestMessageHandlerBase
    {
        private int _expectedMessages;
        private readonly List<object> _receivedMessages;

        protected TestMessageHandlerBase(int  expectedMessages, AutoResetEvent autoResetEvent)
        {
            ExpectedMessages = expectedMessages;
            HandlerId = Guid.NewGuid();
            AutoResetEvent = autoResetEvent;
            _receivedMessages = new List<object>();
        }

        protected TestMessageHandlerBase(int expectedMessages)
            : this(expectedMessages,null)
        {
        }

        protected TestMessageHandlerBase(): this(0, null){}

        public TMessage LastEntityReceived<TMessage>()
        {
            return (TMessage)ReceivedMessages.Last(x=>x.GetType()==typeof(TMessage)); 
        }

        public Guid HandlerId { get; set; }
        public static AutoResetEvent AutoResetEvent { get; set; }
        public void SetReceivedEvent(AutoResetEvent receivedEvent)
        {
            if (receivedEvent == null) throw new ArgumentNullException("receivedEvent");
            AutoResetEvent = receivedEvent;
        }

        public List<object> ReceivedMessages
        {
            get { return _receivedMessages; }
        }

        public int ExpectedMessages
        {
            get { return _expectedMessages; }
            set { _expectedMessages = value; }
        }

        public void Clear()
        {
            ExpectedMessages = 0;
            ReceivedMessages.Clear();
        }

        protected void UpdateHandledMessage(object message)
        {
            ReceivedMessages.Add(message);
            if (ExpectedMessages==ReceivedMessages.Count && AutoResetEvent != null)
                AutoResetEvent.Set();
        }
    }
}
