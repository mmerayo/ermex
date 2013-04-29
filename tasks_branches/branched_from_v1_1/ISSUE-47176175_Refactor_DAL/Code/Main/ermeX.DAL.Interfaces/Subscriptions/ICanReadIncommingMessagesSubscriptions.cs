using System;
using System.Collections.Generic;
using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Subscriptions
{
	internal interface ICanReadIncommingMessagesSubscriptions
	{
		IEnumerable<IncomingMessageSuscription> GetByMessageType(string bizMessageType);
		IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId);
		IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType);
		IEnumerable<IncomingMessageSuscription> FetchAll();
	}
}