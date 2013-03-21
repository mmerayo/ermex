using System;

namespace ermeX.Domain.Queues
{
	interface ICanUpdateOutgoingMessagesQueueInfo
    {
		void RemoveExpiredMessages(TimeSpan expirationTime);
    }
}