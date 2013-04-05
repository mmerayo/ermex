using System;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Domain.Messages;
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
		    ChunkedServiceRequestMessageData result;
		    using (var uow = _factory.Create())
		    {
			    result=_repository.SingleOrDefault(x => x.CorrelationId == correlationId && x.Order == order);
				uow.Commit();
		    }
		    return result;
	    }
    }
}