using System;

namespace ermeX.Domain.Subscriptions
{
	internal interface ICanUpdateIncommingMessagesSubscriptions
	{
		void RemoveByHandlerId(Guid suscriptionId);
		void SaveIncommingSubscription(Guid suscriptionHandlerId, Type handlerType, Type messageType);
	}
}