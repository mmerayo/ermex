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
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Transport.Interfaces.Sending.Client
{
    internal interface IServiceProxy : IDisposable
    {
        ServiceResult Send(TransportMessage message);
        ServiceOperationResult<TResult> SendServiceRequestSync<TResult>(IOperationServiceRequestData request);

        void SendServiceRequestAsync<TResult>(IOperationServiceRequestData request,
                                              Action<IServiceOperationResult<TResult>> responseHandler);
    }
}