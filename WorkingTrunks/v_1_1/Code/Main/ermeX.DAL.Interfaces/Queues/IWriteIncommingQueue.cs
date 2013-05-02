using System.Collections.Generic;
using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Queues
{
	internal interface IWriteIncommingQueue
	{
		//TODO: Create queue interface and call this method as Push
		void Remove(IncomingMessage incomingMessage);
		void Save(IncomingMessage incomingMessage);
		void Save(IEnumerable<IncomingMessage> incomingMessages);
	}
}