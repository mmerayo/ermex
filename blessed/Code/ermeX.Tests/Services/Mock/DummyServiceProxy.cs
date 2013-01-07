// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Messages.ServiceOperations;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Tests.Services.Mock
{
    internal class DummyServiceProxy : IServiceProxy
    {
        public TransportMessage LastSentMessage;
        private int calls;
        public int ForceNumTries { get; set; }

        public int Calls
        {
            get { return calls; }
        }

        public void Dispose()
        {
        }


        public ServiceResult Send(TransportMessage message)
        {
            if (ForceNumTries > 0 && (calls = Calls + 1) < ForceNumTries)
            {
                var serviceResult = new ServiceResult(false);
                serviceResult.ServerMessages.Add("Mock failed server message");
                return serviceResult;
            }
            LastSentMessage = message;
            return new ServiceResult(true);
        }

        public ServiceOperationResult<TResult> SendServiceRequestSync<TResult>(IOperationServiceRequestData request)
        {
            throw new NotImplementedException();
        }

        public void SendServiceRequestAsync<TResult>(IOperationServiceRequestData request,
                                                     Action<IServiceOperationResult<TResult>> responseHandler)
        {
            throw new NotImplementedException();
        }

        public ServiceOperationResult<TResult> SendServiceRequestSync<TResult>(ServiceRequestMessage request)
        {
            throw new NotImplementedException();
        }

        public void SendServiceRequestAsync<TResult>(ServiceRequestMessage request,
                                                     Action<IServiceOperationResult<TResult>> responseHandler)
        {
            throw new NotImplementedException();
        }
    }
}