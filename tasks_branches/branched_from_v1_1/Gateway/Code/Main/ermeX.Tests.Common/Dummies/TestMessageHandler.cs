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
using System.Threading;
using ermeX.Bus.Interfaces;

namespace ermeX.Tests.Common.Dummies
{
    internal class TestMessageHandler : IHandleMessages<DummyDomainEntity>
    {
        private DummyDomainEntity _lastEntityReceived;

        public TestMessageHandler(AutoResetEvent autoResetEvent)
        {
            HandlerId = Guid.NewGuid();
            AutoResetEvent = autoResetEvent;
        }

        public TestMessageHandler() : this(null)
        {
            HandlerId = Guid.NewGuid();
        }

        public DummyDomainEntity LastEntityReceived
        {
            get { return _lastEntityReceived; }
        }

        public Guid HandlerId { get; set; }
        public AutoResetEvent AutoResetEvent { get; set; }


        #region IHandleMessages<DummyDomainEntity> Members

        public void HandleMessage(DummyDomainEntity message)
        {
            _lastEntityReceived = message;
            if (AutoResetEvent != null)
                AutoResetEvent.Set();
        }

        public bool Evaluate(DummyDomainEntity message)
        {
            return true;
        }

        #endregion

        public void Clear()
        {
            _lastEntityReceived = null;
        }
    }
}