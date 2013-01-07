// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Interfaces
{
    internal interface IServiceRequestManager
    {
        void DoRequest(Guid operationIdentifier, Type returnType,
                       Action<IServiceOperationResult<object>> responseHandler, object[] requestParams);

        //ServiceOperationResult<TResult> DoRequest<TResult>(Guid servicePublisher, Guid serviceOperation, object[] requestParams);

        IServiceOperationResult<TResult> DoRequest<TResult>(Tuple<Guid, Guid> serviceOperation, object[] requestParams);
        IServiceOperationResult<object> DoRequest(Type returnType, Tuple<Guid, Guid> requestParams, object[] responseHandler);
    }
}