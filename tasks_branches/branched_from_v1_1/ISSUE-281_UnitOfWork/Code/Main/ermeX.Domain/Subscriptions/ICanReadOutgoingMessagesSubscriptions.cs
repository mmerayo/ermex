using System.Collections.Generic;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Subscriptions
{
	internal interface ICanReadOutgoingMessagesSubscriptions
	{
		IList<OutgoingMessageSuscription> GetByMessageType(string bizMessageType);
		IList<OutgoingMessageSuscription> FetchAll();
	}
}