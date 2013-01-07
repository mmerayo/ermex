// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Transport.Interfaces.Messages.ServiceOperations
{
    public class ServiceOperationResult<TDomain> : IServiceOperationResult<TDomain>
    {
        internal ServiceOperationResult(ServiceResult partialResult)
        {
            OperationResult = partialResult.Ok ? OperationResultType.Success : OperationResultType.Failed;
            ResultValue = (TDomain) partialResult.ResultData;
            InvocationMethod = OperationInvocationMethodType.Synchronous; //TODO:Asynchronous
            if (partialResult.ServerMessages!=null && partialResult.ServerMessages.Count > 0)
                InnerException =
                    new ApplicationException(string.Format("Exception message: {0}{1}", Environment.NewLine,
                                                           string.Join(Environment.NewLine, partialResult.ServerMessages)));
        }

        #region IServiceOperationResult<TDomain> Members

        public OperationResultType OperationResult { get; private set; }

        public TDomain ResultValue { get;  set; }


        public OperationInvocationMethodType InvocationMethod { get; private set; }

        public Exception InnerException { get; private set; }

        #endregion
    }
}