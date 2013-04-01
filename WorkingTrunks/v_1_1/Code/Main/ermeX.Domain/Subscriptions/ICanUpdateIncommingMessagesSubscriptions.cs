using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Subscriptions
{
	internal interface ICanUpdateIncommingMessagesSubscriptions
	{
		void RemoveByHandlerId(Guid suscriptionId);
		void SaveIncommingSubscription(Guid suscriptionHandlerId, Type handlerType, Type messageType);
		void Remove(List<IncomingMessageSuscription> toRemove);
	}
}