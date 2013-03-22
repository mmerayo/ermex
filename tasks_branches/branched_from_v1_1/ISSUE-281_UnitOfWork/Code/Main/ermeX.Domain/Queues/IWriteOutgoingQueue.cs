using System;

namespace ermeX.Domain.Queues
{
	interface IWriteOutgoingQueue
    {
		void RemoveExpiredMessages(TimeSpan expirationTime);
    }
}