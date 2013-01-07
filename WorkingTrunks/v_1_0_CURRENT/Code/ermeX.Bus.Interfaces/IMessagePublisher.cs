// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Interfaces;
using ermeX.LayerMessages;

namespace ermeX.Bus.Interfaces
{
    internal interface IMessagePublisher 
    {
        void PublishMessage(BusMessage message);
        // void PublishMessage<TMessage>(MessagePriority priority, TMessage message) where TMessage:class;
        void Start();

        TServiceInterface GetServiceProxy<TServiceInterface>() where TServiceInterface : IService;

        /// <summary>
        ///   When there are several components publishing the same sevice, i.e. system services it specifies concretely which component to get the proxy for
        /// </summary>
        /// <typeparam name="TServiceInterface"> </typeparam>
        /// <param name="componentId"> </param>
        /// <returns> </returns>
        TServiceInterface GetServiceProxy<TServiceInterface>(Guid componentId) where TServiceInterface : IService;
    }
}