using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Queues;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Queues
{
	class IncommingQueueWriter : IWriteIncommingQueue
	{
		private readonly IPersistRepository<IncomingMessage> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public IncommingQueueWriter(IPersistRepository<IncomingMessage> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
        {
	        _repository = repository;
	        _factory = factory;
        }

		public void Save(IncomingMessage incomingMessage)
		{
			using (var uow = _factory.Create())
			{
				_repository.Save(incomingMessage);
				uow.Commit();
			}
		}

		public void Save(IEnumerable<IncomingMessage> incomingMessages)
		{
			using (var uow = _factory.Create())
			{
				_repository.Save(incomingMessages);
				uow.Commit();
			}
		}

		public void Remove(IncomingMessage incomingMessage)
		{
			using (var uow = _factory.Create())
			{
				_repository.Remove(incomingMessage);
				uow.Commit();
			}
			
		}

	}
}
