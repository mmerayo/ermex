// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Biz.Interfaces
{
    internal interface ISubscriptionsManager
    {
        /// <summary>
        ///   suscribes to one message
        /// </summary>
        /// <returns> The suscriptionHandlerId </returns>
        TResult Subscribe<TResult>(Type handlerType);
       
        object Subscribe(Type handlerType);
        TResult Subscribe<TResult>(Type handlerType,Type messageType);
        object Subscribe(Type handlerType, Type messageType);
    }
}