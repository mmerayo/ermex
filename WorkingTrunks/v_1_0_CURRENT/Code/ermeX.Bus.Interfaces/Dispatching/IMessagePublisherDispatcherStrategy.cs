// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;

namespace ermeX.Bus.Interfaces.Dispatching
{
    /// <summary>
    ///   Interface for a dispatcher strategy
    /// </summary>
    internal interface IMessagePublisherDispatcherStrategy : IDisposable
    {
        /// <summary>
        ///   Status of the dispatcher
        /// </summary>
        DispatcherStatus Status { get; }

        /// <summary>
        ///   Dispatches one message publishing it
        /// </summary>
        /// <param name="messageToPublish"> </param>
        void Dispatch(BusMessage messageToPublish);

        /// <summary>
        ///   It starts the strategy
        /// </summary>
        void Start();

        /// <summary>
        ///   It stops the strategy
        /// </summary>
        void Stop();
    }
}