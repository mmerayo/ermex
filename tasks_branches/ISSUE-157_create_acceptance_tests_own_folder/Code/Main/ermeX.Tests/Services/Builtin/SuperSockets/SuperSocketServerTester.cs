// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Mock;

using ermeX.Transport.BuiltIn.SuperSocket.Server;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Receiving.Server;

namespace ermeX.Tests.Services.Builtin.SuperSockets
{
    internal class SuperSocketServerTester : ServerTesterBase
    {
        protected override IServer GetServerInstance(ServerInfo serverInfo)
        {
            var settings =
                (TestSettingsProvider.ClientSettings) TestSettingsProvider.GetClientConfigurationSettingsSource();
            settings.MaxMessageKbBeforeChunking = 8192;
            return new SuperSocketServer(serverInfo, ServiceDetailsDs,null, settings);
        }

        protected override IMockTestClient GetTestClientInstance(ServerInfo serverInfo)
        {
            return new DummyTestSuperSocketClient(serverInfo);
        }
    }
}