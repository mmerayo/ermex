using System;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Queues;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Queues
{
	internal class WriteOutgoingQueue : IWriteOutgoingQueue
	{
		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<OutgoingMessage> _repository;

		[Inject]
		public WriteOutgoingQueue(IPersistRepository<OutgoingMessage> repository, 
			IUnitOfWorkFactory factory, 
			IComponentSettings settings)
		{
			_factory = factory;
			_repository = repository;
		}

		public void RemoveExpiredMessages(TimeSpan expirationTime)
		{
			using (var uow = _factory.Create())
			{
				DateTime dateTime = DateTime.UtcNow - expirationTime;
				_repository.Remove(uow, x => x.CreatedTimeUtc <= dateTime);
				uow.Commit();
			}
		}

		public void Save(OutgoingMessage message)
		{
			using (var uow = _factory.Create())
			{
				_repository.Save(uow, message);
				uow.Commit();
			}
		}
	}
}