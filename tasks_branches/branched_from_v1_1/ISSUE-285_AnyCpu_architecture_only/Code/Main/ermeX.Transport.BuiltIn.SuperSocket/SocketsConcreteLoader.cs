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
using Ninject;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;


using ermeX.Entities.Entities;
using ermeX.Transport.BuiltIn.SuperSocket.Client;
using ermeX.Transport.BuiltIn.SuperSocket.Server;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Transport.BuiltIn.SuperSocket
{
    internal class SocketsConcreteLoader : LoaderBase
    {
        private IChunkedServiceRequestMessageDataSource ChunkedServiceRequestMessageDataSource { get; set; }

        [Inject]
        public SocketsConcreteLoader(ITransportSettings settings, IConnectivityDetailsDataSource connectionsDs,
                                     IServiceDetailsDataSource servicesDs,
                                     ICacheProvider cacheProvider, IChunkedServiceRequestMessageDataSource chunkedServiceRequestMessageDataSource)
            : base(settings, connectionsDs, servicesDs, cacheProvider)
        {
            ChunkedServiceRequestMessageDataSource = chunkedServiceRequestMessageDataSource;
        }

        protected override IServer GetServer(ConnectivityDetails item)
        {
            return new SuperSocketServer(new ServerInfo(item), ServicesDs,ChunkedServiceRequestMessageDataSource,  Settings);
        }


        protected override int GetPort(ConnectivityDetails item)
        {
            return item.Port;
        }

        protected override IEndPoint GetClientConcreteProxy(Guid serverId, List<ServerInfo> serverInfos)
        {
            return new SuperSocketClient(CacheProvider, Settings, serverInfos);
        }
    }
}