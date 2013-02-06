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
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.Entities.Entities;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyEsbSettings : IBusSettings
    {
        public IEsbManager BusManager { get; set; }

        public IMessagePublisherDispatcherStrategy MessageDispatcher { get; set; }

       

        public int MaxDelayDueToLatencySeconds
        {
            get { throw new NotImplementedException(); }
        }

       
        public IEnumerable<AppComponent> SuscribersToLocalMessages
        {
            get { throw new NotImplementedException(); }
        }


        public Guid ComponentId { get; set; }

        public int CacheExpirationSeconds { get; set; }

        public Type ConfigurationManagerType
        {
            get { throw new NotImplementedException(); }
        }

        public bool DevLoggingActive
        {
            get { return true; }
        }

        public bool InternalDiagnosticsActive { get; set; }


        public TimeSpan SendExpiringTime
        {
            get { throw new NotImplementedException(); }
        }

        public NetworkingMode NetworkingMode
        {
            get { throw new NotImplementedException(); }
        }

        public FriendComponentData FriendComponent
        {
            get { throw new NotImplementedException(); }
        }
    }
}