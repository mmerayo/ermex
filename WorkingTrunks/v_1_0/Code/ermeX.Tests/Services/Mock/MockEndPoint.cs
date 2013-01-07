// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Tests.Services.Mock
{
    internal class MockEndPoint : IEndPoint
    {
        public readonly List<object> MessagesSent = new List<object>();
        public bool Fails;
        public bool RaisesException;

        public void Dispose()
        {
        }

        public ServiceResult Send(ServiceRequestMessage message)
        {
            if (RaisesException)
                throw new Exception("Failure for testing");
            MessagesSent.Add(message);
            return new ServiceResult(!Fails);
        }
    }
}