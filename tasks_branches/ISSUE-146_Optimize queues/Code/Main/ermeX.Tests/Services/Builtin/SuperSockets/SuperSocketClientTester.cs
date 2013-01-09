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
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Tests.Services.Mock;
using ermeX.Tests.Services.Sending;
using ermeX.Transport.BuiltIn.SuperSocket.Client;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Tests.Services.Builtin.SuperSockets
{
    internal class SuperSocketClientTester : ServiceClientTesterBase
    {
        protected override MockTestServerBase<TMessage> GetDummyTestServer<TMessage>(bool local,
                                                                                     AutoResetEvent eventDone,
                                                                                     int freePort, Guid s)
        {
            var res = new DummyTestSuperSocketServer<TMessage>(freePort);
            res.Init(eventDone);

            return res;
        }

        protected override IEndPoint GetClient(MemoryCacheStore cacheProvider, ITransportSettings settings,
                                               List<ServerInfo> serverInfos)
        {
            return new SuperSocketClient(cacheProvider, settings, serverInfos);
        }
    }
}