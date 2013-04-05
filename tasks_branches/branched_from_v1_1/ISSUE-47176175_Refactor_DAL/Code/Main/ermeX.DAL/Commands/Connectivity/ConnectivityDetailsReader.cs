using System;
using Ninject;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Domain.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Connectivity
{
    internal class ConnectivityDetailsReader : ICanReadConnectivityDetails
    {
	    private readonly IReadOnlyRepository<ConnectivityDetails> _repository;
	    private readonly IUnitOfWorkFactory _factory;

	    [Inject]
        public ConnectivityDetailsReader(IReadOnlyRepository<ConnectivityDetails> repository, 
			IUnitOfWorkFactory factory)
        {
	        _repository = repository;
	        _factory = factory;
        }

	    public ConnectivityDetails Fetch(Guid componentId)
	    {
		    ConnectivityDetails result;
		    using (var uow = _factory.Create())
		    {
			    result=_repository.SingleOrDefault(x=>x.ServerId==componentId);
				uow.Commit();
		    }
		    return result;
	    }
    }
}