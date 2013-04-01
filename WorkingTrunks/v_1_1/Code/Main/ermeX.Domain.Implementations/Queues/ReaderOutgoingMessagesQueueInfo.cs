using System;
using System.Collections.Generic;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Queues;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Queues
{
	class ReaderOutgoingQueue : IReadOutgoingQueue
	{
		private IOutgoingMessagesDataSource Repository { get; set; }

        [Inject]
		public ReaderOutgoingQueue(IOutgoingMessagesDataSource repository)
        {
            Repository = repository;
        }

		public IEnumerable<OutgoingMessage> GetItemsPendingSorted()
		{
			return Repository.GetItemsPendingSorted();//TODO: MOVE LOGIC HERE
		}

		public OutgoingMessage GetNextDeliverable()
		{
			return Repository.GetNextDeliverable();//TODO: MOVE LOGIC HERE
		}

		public OutgoingMessage GetByBusMessageId(int id)
		{
			return Repository.GetByBusMessageId(id);//TODO: MOVE LOGIC HERE
		}

		public IEnumerable<OutgoingMessage> GetExpiredMessages(TimeSpan expirationTime)
		{
			return Repository.GetExpiredMessages(expirationTime);//TODO: MOVE LOGIC HERE
		}


		public bool ContainsMessageFor(Guid messageId, Guid destinationComponent)
		{
			return Repository.ContainsMessageFor(messageId, destinationComponent);//TODO: MOVE LOGIC HERE
		}

		public IEnumerable<OutgoingMessage> GetByStatus(params Message.MessageStatus[] status)
		{
			return Repository.GetByStatus(status);//TODO: MOVE LOGIC HERE
		}
	}
}