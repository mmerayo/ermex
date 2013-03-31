using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Queues;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Queues
{
	class IncommingQueueWriter : IWriteIncommingQueue
	{
		private IIncomingMessagesDataSource Repository { get; set; }

        [Inject]
		public IncommingQueueWriter(IIncomingMessagesDataSource repository)
        {
            Repository = repository;
        }

		public void Save(IncomingMessage incomingMessage)
		{
			Repository.Save(incomingMessage);
		}

		public void Save(IEnumerable<IncomingMessage> incomingMessages)
		{
			Repository.Save(incomingMessages);
		}

		public void Remove(IncomingMessage incomingMessage)
		{
			Repository.Remove(incomingMessage);
		}

	}
}
