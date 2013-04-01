using System;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Queues
{
	interface IWriteOutgoingQueue
    {
		void RemoveExpiredMessages(TimeSpan expirationTime);
		void Save(OutgoingMessage result);
    }
}