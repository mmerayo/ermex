// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Interfaces;

namespace ermeX.Bus.Interfaces
{
    //TODO: sagas with configurator

    internal interface IMessageListener
    {
        /// <summary>
        ///   suscribes to one message
        /// </summary>
        /// <returns> The suscriptionHandlerId </returns>
        Guid Suscribe(Type handlerInterfaceType, object handler);


        /// <summary>
        ///   suscribes to one message
        /// </summary>
        
        /// <param name="handlerInterfaceType"> </param>
        /// <param name="handler"> handler to subscribe </param>
        /// <param name="objHandler"> real subscribed handler </param>
        /// <returns> The suscriptionHandlerId </returns>
        Guid Suscribe(Type handlerInterfaceType, object handler, out object objHandler);

        // void Unscribe(Guid suscriptionHandlerId);
        void Start();
        void PublishService<TServiceInterface>(Type serviceImplementation) where TServiceInterface : IService;

        /// <summary>
        ///   Publishes the IoC injected implementation
        /// </summary>
        /// <typeparam name="TServiceInterface"> </typeparam>
        void PublishService<TServiceInterface>() where TServiceInterface : IService;

        void PublishService(Type serviceInterface, Type serviceImplementation);
    }
}