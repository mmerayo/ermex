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
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyClientConfigurationSettings : ITransportSettings,IBusSettings
    {

        public Guid ComponentId { get; set; }

        public int CacheExpirationSeconds { get; set; }

        public NetworkingMode NetworkingMode
        {
            get { throw new NotImplementedException(); }
        }

        public FriendComponentData FriendComponent
        {
            get { throw new NotImplementedException(); }
        }

        public IEsbManager BusManager
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IMessagePublisherDispatcherStrategy MessageDispatcher
        {
            get { throw new NotImplementedException(); }
        }

        

        public TimeSpan SendExpiringTime { get; set; }



        public int MaxDelayDueToLatencySeconds
        {
            get { throw new NotImplementedException(); }
        }

      

        public int MaxMessageKbBeforeChunking { get; set; }

        public ushort TcpPort
        {
            get { throw new NotImplementedException(); }
        }

        public ushort MaxPortRange
        {
            get { throw new NotImplementedException(); }
        }

        public Type ConfigurationManagerType { get; set; }

        public bool DevLoggingActive
        {
            get { return true; }
        }

        public bool InternalDiagnosticsActive { get; set; }

       
    }
}