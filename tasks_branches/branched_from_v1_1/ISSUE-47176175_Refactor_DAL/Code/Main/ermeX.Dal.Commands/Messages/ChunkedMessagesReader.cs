using System;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Repository;

using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Messages;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Messages
{
    internal sealed class ChunkedMessagesReader : ICanReadChunkedMessages
    {
	    private readonly IReadOnlyRepository<ChunkedServiceRequestMessageData> _repository;
	    private readonly IUnitOfWorkFactory _factory;

	    [Inject]
		public ChunkedMessagesReader(IReadOnlyRepository<ChunkedServiceRequestMessageData> repository,
			IUnitOfWorkFactory factory, IComponentSettings settings)
        {
	        _repository = repository;
	        _factory = factory;
        }

	    public ChunkedServiceRequestMessageData Fetch(Guid correlationId, int order)
	    {
		    ChunkedServiceRequestMessageData result = null;
			_factory.ExecuteInUnitOfWork(true,
			    uow => result = _repository.SingleOrDefault(uow, x => x.CorrelationId == correlationId && x.Order == order));
		    
		    return result;
	    }
    }
}