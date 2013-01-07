// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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