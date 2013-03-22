using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Queues;

namespace ermeX.Domain.Implementations.Queues
{
	internal class WriteOutgoingQueue : IWriteOutgoingQueue
	{
		private IOutgoingMessagesDataSource Repository { get; set; }

		[Inject]
		public WriteOutgoingQueue(IOutgoingMessagesDataSource repository)
		{
			Repository = repository;
		}

		public void RemoveExpiredMessages(TimeSpan expirationTime)
		{
			Repository.RemoveExpiredMessages(expirationTime); //TODO: MOVE LOGIC HERE
		}
	}
}