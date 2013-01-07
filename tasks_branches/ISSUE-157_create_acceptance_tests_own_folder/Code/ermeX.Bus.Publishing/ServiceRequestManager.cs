// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Common;
using ermeX.Transport.Interfaces.Messages.ServiceOperations;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Publishing
{
    internal class ServiceRequestManager : BusInteropBase, IServiceRequestManager
    {
        [Inject]
        public ServiceRequestManager(IEsbManager bus)
            : base(bus)
        {
        }

        #region IServiceRequestManager Members

        public IServiceOperationResult<TResult> DoRequest<TResult>(Tuple<Guid, Guid> serviceOperation,
                                                                   object[] requestParams)
        {
            return DoRequest<TResult>(serviceOperation.Item1, serviceOperation.Item2, requestParams);
        }

        public IServiceOperationResult<object> DoRequest(Type returnType, Tuple<Guid, Guid> requestParams, object[] responseHandler)
        {
            var serviceOperationResult = (ServiceOperationResult<object>)DoRequest<object>(requestParams, responseHandler);
            if (serviceOperationResult.ResultValue != null)
            {
                if (serviceOperationResult.ResultValue is string && returnType != typeof (string))
                    //TODO:REMOVE WHEN CHANGED SERIALIZATION
                    serviceOperationResult.ResultValue = TypesHelper.ConvertFrom(returnType.FullName,
                                                                                 serviceOperationResult.ResultValue);
                else if (returnType.IsEnum)
                    serviceOperationResult.ResultValue = TypesHelper.ConvertFrom(returnType.FullName,
                                                                                 serviceOperationResult.ResultValue.ToString());
            }
            return serviceOperationResult;
        }

        public void DoRequest(Guid operationIdentifier, Type returnType,
                              Action<IServiceOperationResult<object>> responseHandler, object[] requestParams)
        {
            DoRequest(new Tuple<Guid, Guid>(Guid.Empty, operationIdentifier), returnType, responseHandler, requestParams);
        }

        #endregion

        public IServiceOperationResult<TResult> DoRequest<TResult>(Guid destinationComponent, Guid serviceOperation,
                                                                   object[] requestParams)
        {
            if (serviceOperation == Guid.Empty)
                throw new ArgumentException("Cannot be an empty value", "serviceOperation");


            return Bus.RequestService<TResult>(destinationComponent, serviceOperation, requestParams);
        }

        public void DoRequest(Tuple<Guid, Guid> serviceOperation, Type returnType,
                              Action<IServiceOperationResult<object>> responseHandler, object[] requestParams)
        {
            if (serviceOperation.Item2 == Guid.Empty)
                throw new ArgumentException("Cannot be an empty value", "serviceOperation");


            Bus.RequestService(serviceOperation.Item1, serviceOperation.Item2, responseHandler, requestParams);
        }
    }
}