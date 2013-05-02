using System.Collections.Generic;
using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Subscriptions
{
	internal interface ICanReadOutgoingMessagesSubscriptions
	{
		IEnumerable<OutgoingMessageSuscription> GetByMessageType(string bizMessageType);
		IEnumerable<OutgoingMessageSuscription> FetchAll();
	}
}