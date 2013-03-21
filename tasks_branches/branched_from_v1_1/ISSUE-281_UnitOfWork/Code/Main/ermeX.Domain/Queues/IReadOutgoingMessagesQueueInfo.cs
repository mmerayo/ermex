using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Queues
{
	interface IReadOutgoingMessagesQueueInfo
	{
		IEnumerable<OutgoingMessage> GetItemsPendingSorted();
		OutgoingMessage GetNextDeliverable();
		OutgoingMessage GetByBusMessageId(int id);
		IEnumerable<OutgoingMessage> GetExpiredMessages(TimeSpan expirationTime);
		
		bool ContainsMessageFor(Guid messageId, Guid destinationComponent);
		IEnumerable<OutgoingMessage> GetByStatus(params Message.MessageStatus[] status);
	}
}
