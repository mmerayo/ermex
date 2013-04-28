using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Messages;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Entities.Entities;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.DAL.Commands.Messages
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
			_factory.ExecuteInUnitOfWork(false,uow =>_repository.Save(uow, chunk));
		}

		public void Remove(ChunkedServiceRequestMessage chunk)
		{
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Remove(uow, chunk));
		}
	}
}