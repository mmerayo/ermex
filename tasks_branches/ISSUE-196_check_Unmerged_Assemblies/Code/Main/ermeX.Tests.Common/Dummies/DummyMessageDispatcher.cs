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
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.LayerMessages;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyMessageDispatcher : IMessagePublisherDispatcherStrategy
    {
        public BusMessage LastPublishedMessage { get; private set; }

        #region IMessagePublisherDispatcherStrategy Members

        public void Dispose()
        {
        }

        public DispatcherStatus Status { get; set; }

        

        public void Start()
        {
            Status = DispatcherStatus.Started;
        }

        public void Stop()
        {
            Status = DispatcherStatus.Stopped;
        }

        #endregion


        public void Dispatch(BusMessage messageToPublish)
        {
            LastPublishedMessage = messageToPublish;
        }

        public void Clear()
        {
            Status = DispatcherStatus.Stopped;
            LastPublishedMessage = null;
        }
    }
}