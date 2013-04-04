using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Messages;
using ermeX.Entities.Entities;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Domain.Implementations.Messages
{
	class ChunkedMessagesWriter : ICanWriteChunkedMessages
	{
		private readonly IPersistRepository<ChunkedServiceRequestMessageData> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public ChunkedMessagesWriter(IPersistRepository<ChunkedServiceRequestMessageData> repository,
			IUnitOfWorkFactory factory, IComponentSettings settings)
        {
	        _repository = repository;
	        _factory = factory;
        }

		public void Save(ChunkedServiceRequestMessage chunk)
		{
			using (var uow = _factory.Create())
			{
				_repository.Save(chunk);
				uow.Commit();
			}
		}

		public void Remove(ChunkedServiceRequestMessage chunk)
		{
			using (var uow = _factory.Create())
			{
				_repository.Remove(chunk);
				uow.Commit();
			}

		}
	}
}