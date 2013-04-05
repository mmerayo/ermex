using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Subscriptions
{
	internal interface ICanReadOutgoingMessagesSubscriptions
	{
		IEnumerable<OutgoingMessageSuscription> GetByMessageType(string bizMessageType);
		IEnumerable<OutgoingMessageSuscription> FetchAll();
	}
}