// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System;
using ermeX.Bus.Interfaces;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Messages.ServiceOperations;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Tests.Common.Dummies
{
    internal class MockBusProvider : IEsbManager
    {
        

        public void Publish(BusMessage message)
        {
        }

        public void Start()
        {
        }

        public IServiceOperationResult<TResult> RequestService<TResult>(Guid destinationComponent, Guid serviceOperation, object[] requestParams)
        {
            throw new NotImplementedException();
        }

        public void RequestService<TResult>(Guid destinationComponent, Guid serviceOperation, Action<IServiceOperationResult<TResult>> responseHandler, object[] requestParams)
        {
            throw new NotImplementedException();
        }

        public ServiceOperationResult<TResult> RequestService<TResult>(Guid serviceOperation, object[] requestParams)
        {
            throw new NotImplementedException();
        }

        public void RequestService<TResult>(Guid serviceOperation,
                                            Action<IServiceOperationResult<TResult>> responseHandler,
                                            object[] requestParams)
        {
            throw new NotImplementedException();
        }
    }
}