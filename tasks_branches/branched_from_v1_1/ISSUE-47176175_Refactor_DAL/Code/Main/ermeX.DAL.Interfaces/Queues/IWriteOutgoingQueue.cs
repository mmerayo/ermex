using System;
using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Queues
{
	interface IWriteOutgoingQueue
    {
		void RemoveExpiredMessages(TimeSpan expirationTime);
		void Save(OutgoingMessage result);
    }
}