using System;
using System.Collections.Generic;
using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Queues
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
