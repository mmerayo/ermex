// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Interfaces
{

    //TODO: REMOVE THIS CLASS
    /// <summary>
    ///   Interface for the EsbManager
    /// </summary>
    internal interface IEsbManager
    {
        /// <summary>
        ///   Publishes one message 
        /// </summary>
        /// <param name="message"> </param>
        void Publish(BusMessage message);

        /// <summary>
        ///   Starts the subcomponent
        /// </summary>
        void Start();

        /// <summary>
        ///   Requests a service. It doesnt handle the response
        /// </summary>
        /// <typeparam name="TResult"> </typeparam>
        /// <param name="destinationComponent"> </param>
        /// <param name="serviceOperation"> </param>
        /// <param name="requestParams"> </param>
        /// <returns> </returns>
        IServiceOperationResult<TResult> RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
                                                                 object[] requestParams);

        /// <summary>
        ///   Requests a service. It handles the response
        /// </summary>
        /// <typeparam name="TResult"> </typeparam>
        /// <param name="destinationComponent"> </param>
        /// <param name="serviceOperation"> </param>
        /// <param name="responseHandler"> </param>
        /// <param name="requestParams"> </param>
        void RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
                                     Action<IServiceOperationResult<TResult>> responseHandler, object[] requestParams);
    }
}