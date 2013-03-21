using System;
using System.Collections.Generic;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Queues;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Queues
{
	class ReaderIncommingMessagesQueueInfo : IReadIncommingMessagesQueueInfo
	{
		private IIncomingMessagesDataSource Repository { get; set; }

        [Inject]
		public ReaderIncommingMessagesQueueInfo(IIncomingMessagesDataSource repository)
        {
            Repository = repository;
        }

		public IncomingMessage GetNextDispatchableItem(int maxLatency)
		{
			return Repository.GetNextDispatchableItem(maxLatency);//TODO: MOVE LOGIC HERE
		}

		public IEnumerable<IncomingMessage> GetMessagesToDispatch()
		{
			return Repository.GetMessagesToDispatch();//TODO: MOVE LOGIC HERE
		}

		public IEnumerable<IncomingMessage> GetByStatus(params Message.MessageStatus[] status)
		{
			return Repository.GetByStatus(status);//TODO: MOVE LOGIC HERE
		}

		public bool ContainsMessageFor(Guid messageId, Guid destination)
		{
			return Repository.ContainsMessageFor(messageId,destination);//TODO: MOVE LOGIC HERE
		}

		public IEnumerable<IncomingMessage> GetNonDistributedMessages()
		{
			return Repository.GetNonDistributedMessages();//TODO: MOVE LOGIC HERE
		}
	}
}