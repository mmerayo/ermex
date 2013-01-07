// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Publishing.Dispatching
{
    internal class ServiceRequestDispatcher : IServiceRequestDispatcher
    {
        [Inject]
        public ServiceRequestDispatcher(IServiceProxy proxy)
        {
            if (proxy == null) throw new ArgumentNullException("proxy");
            ServiceProxy = proxy;
        }

        private IServiceProxy ServiceProxy { get; set; }

        #region IServiceRequestDispatcher Members

        public IServiceOperationResult<TResult> RequestSync<TResult>(ServiceRequestMessage request)
        {
            return ServiceProxy.SendServiceRequestSync<TResult>(request);
        }

        public void RequestAsync<TResult>(ServiceRequestMessage request,
                                          Action<IServiceOperationResult<TResult>> responseHandler)
        {
            ServiceProxy.SendServiceRequestAsync(request, responseHandler);
        }

        #endregion
    }
}