// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Transport.LayerInterfaces
{
    internal interface IConcreteServiceLoader : IDisposable
    {
        IEnumerable<IServer> LoadServers();
        IEndPoint GetClientProxy(Guid destinationComponent);
    }
}