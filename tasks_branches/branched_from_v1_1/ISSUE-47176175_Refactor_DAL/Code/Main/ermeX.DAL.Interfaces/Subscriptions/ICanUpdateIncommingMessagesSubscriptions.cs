using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Subscriptions
{
	internal interface ICanUpdateIncommingMessagesSubscriptions
	{
		void RemoveByHandlerId(Guid suscriptionId);
		void SaveIncommingSubscription(Guid suscriptionHandlerId, Type handlerType, Type messageType);
		void Remove(IEnumerable<IncomingMessageSuscription> toRemove);
	}
}