using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Queues;

namespace ermeX.Domain.Implementations.Queues
{
	internal class CanUpdateOutgoingMessagesQueueInfo : ICanUpdateOutgoingMessagesQueueInfo
	{
		private IOutgoingMessagesDataSource Repository { get; set; }

		[Inject]
		public CanUpdateOutgoingMessagesQueueInfo(IOutgoingMessagesDataSource repository)
		{
			Repository = repository;
		}

		public void RemoveExpiredMessages(TimeSpan expirationTime)
		{
			Repository.RemoveExpiredMessages(expirationTime); //TODO: MOVE LOGIC HERE
		}
	}
}