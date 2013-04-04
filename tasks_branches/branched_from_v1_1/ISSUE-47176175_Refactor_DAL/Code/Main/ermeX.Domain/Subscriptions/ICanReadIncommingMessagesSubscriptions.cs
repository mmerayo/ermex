using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Subscriptions
{
	internal interface ICanReadIncommingMessagesSubscriptions
	{
		IEnumerable<IncomingMessageSuscription> GetByMessageType(string bizMessageType);
		IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId);
		IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType);
		IEnumerable<IncomingMessageSuscription> FetchAll();
	}
}