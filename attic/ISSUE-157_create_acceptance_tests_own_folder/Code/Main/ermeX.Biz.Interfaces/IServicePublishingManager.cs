// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Interfaces;

namespace ermeX.Biz.Interfaces
{
    internal interface IServicePublishingManager
    {
        void PublishService<TServiceInterface>(Type serviceImplementation) where TServiceInterface : IService;

        /// <summary>
        ///   Publishes the IoC injected implementation
        /// </summary>
        /// <typeparam name="TServiceInterface"> </typeparam>
        void PublishService<TServiceInterface>() where TServiceInterface : IService;

        void PublishService(Type serviceInterface, Type serviceImplementation);
    }
}