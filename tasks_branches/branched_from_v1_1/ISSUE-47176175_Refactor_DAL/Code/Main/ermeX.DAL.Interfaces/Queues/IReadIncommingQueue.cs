using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Queues
{
	interface IReadIncommingQueue
	{
		IncomingMessage GetNextDispatchableItem(int maxLatency);
		IEnumerable<IncomingMessage> GetMessagesToDispatch();
		IEnumerable<IncomingMessage> GetByStatus(params Message.MessageStatus[] status);
		bool ContainsMessageFor(Guid messageId, Guid destinationComponent); //TODO: NOW IS THE SUSCRIPTOR ID BUT IT MUST BE THE QUEUE
		IEnumerable<IncomingMessage> GetNonDistributedMessages();
	}
}
