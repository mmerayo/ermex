// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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