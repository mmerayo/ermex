using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Subscriptions
{
	internal interface ICanReadIncommingMessagesSubscriptions
	{
		IList<IncomingMessageSuscription> GetByMessageType(string bizMessageType);
		IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId);
		IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType);
		IList<IncomingMessageSuscription> FetchAll();
	}
}