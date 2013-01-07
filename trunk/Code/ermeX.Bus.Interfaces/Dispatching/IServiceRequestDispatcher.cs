// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Interfaces.Dispatching
{
    /// <summary>
    ///   Interface for the service requests dispatcher
    /// </summary>
    internal interface IServiceRequestDispatcher
    {
        /// <summary>
        ///   Request a service operation synchronously
        /// </summary>
        /// <typeparam name="TResult"> </typeparam>
        /// <param name="request"> </param>
        /// <returns> </returns>
        IServiceOperationResult<TResult> RequestSync<TResult>(ServiceRequestMessage request);

        /// <summary>
        ///   Requests a service operation asynchronously
        /// </summary>
        /// <typeparam name="TResult"> </typeparam>
        /// <param name="request"> </param>
        /// <param name="responseHandler"> </param>
        void RequestAsync<TResult>(ServiceRequestMessage request,
                                   Action<IServiceOperationResult<TResult>> responseHandler);
    }
}