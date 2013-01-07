// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Tests.Services.Mock
{
    internal class MockConnectivityProvider : IConnectivityManager
    {
        public readonly List<IEndPoint> ClientProxiesForComponent = new List<IEndPoint>();

        public MockConnectivityProvider(bool hasEndPoints)
        {
            if (hasEndPoints)
            {
                ClientProxiesForComponent.Add(new MockEndPoint());
                ClientProxiesForComponent.Add(new MockEndPoint());
            }
        }

        public void Dispose()
        {
        }

        public List<IEndPoint> GetClientProxiesForComponent(Guid destinationComponent)
        {
            return ClientProxiesForComponent;
        }

        public void LoadServers()
        {
            throw new NotImplementedException();
        }

        public void RegisterHandler(Guid operationIdentifier, IServiceHandler internalMessageHandler)
        {
            throw new NotImplementedException();
        }

        public void StartListening()
        {
            throw new NotImplementedException();
        }

        public void StartServers(Action<Guid, object> messageReceivedHandler)
        {
            throw new NotImplementedException();
        }
    }
}