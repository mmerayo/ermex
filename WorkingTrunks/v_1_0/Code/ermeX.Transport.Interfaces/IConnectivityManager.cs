// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Transport.Interfaces
{
    internal interface IConnectivityManager : IDisposable
    {
        List<IEndPoint> GetClientProxiesForComponent(Guid destinationComponent);

        void LoadServers();
        void RegisterHandler(Guid operationIdentifier, IServiceHandler internalMessageHandler);
        void StartListening();
    }
}