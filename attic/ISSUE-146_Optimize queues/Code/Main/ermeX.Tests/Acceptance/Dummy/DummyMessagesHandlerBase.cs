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

namespace ermeX.Tests.Acceptance.Dummy
{
    [Serializable]
    internal class DummyMessagesHandlerBase<TEntity> : MarshalByRefObject
    {
        public readonly List<TEntity> ReceivedMessages = new List<TEntity>();
        private AutoResetEvent _finishedEvent;
        private int _numMessages;

        public void HandleMessage(TEntity message)
        {
            ReceivedMessages.Add(message);
            Console.WriteLine("Received message of type: {0}", typeof(TEntity).FullName);
            if (ReceivedMessages.Count == _numMessages)
                _finishedEvent.Set();
        }

        public bool Evaluate(TEntity message)
        {
            return true;
        }

        public void NotifyWhenReceive(int numMessages, AutoResetEvent finishedEvent)
        {
            _numMessages = numMessages;
            _finishedEvent = finishedEvent;

            if (_numMessages == 0)
                finishedEvent.Set();
        }
    }
}