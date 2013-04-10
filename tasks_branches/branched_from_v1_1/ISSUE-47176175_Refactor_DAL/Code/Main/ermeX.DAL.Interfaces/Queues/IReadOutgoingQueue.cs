using System;
using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces.Queues
{
	interface IReadOutgoingQueue
	{
		IEnumerable<OutgoingMessage> GetItemsPendingSorted();
		IEnumerable<OutgoingMessage> GetExpiredMessages(TimeSpan expirationTime);
		
		bool ContainsMessageFor(Guid messageId, Guid destinationComponent);
		IEnumerable<OutgoingMessage> GetByStatus(params Message.MessageStatus[] status);
	}
}
