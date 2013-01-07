// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces
{
    internal interface IIncomingMessageSuscriptionsDataSource : IDataSource<IncomingMessageSuscription>
    {
        IList<IncomingMessageSuscription> GetByMessageType(string bizMessageType);
        IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId);
        void RemoveByHandlerId(Guid suscriptionId);
        IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType,Type messageType);

        void SaveIncommingSubscription(Guid suscriptionHandlerId, Type handlerType, Type messageType);
    }
}