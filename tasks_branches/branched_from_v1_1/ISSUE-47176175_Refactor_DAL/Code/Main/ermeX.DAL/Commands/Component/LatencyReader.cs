using Ninject;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Component;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Component
{
    internal sealed class LatencyReader : ICanReadLatency
    {
	    private readonly IUnitOfWorkFactory _factory;
	    private IReadOnlyRepository<AppComponent> Repository { get; set; }

        [Inject]
        public LatencyReader(IReadOnlyRepository<AppComponent> repository, IUnitOfWorkFactory factory)
        {
	        _factory = factory;
	        Repository = repository;
        }

	    public int GetMaxLatency()
	    {
		    int result=0;
		    using (var u = _factory.Create())
		    {
			     result=Repository.GetMax<int>(u.Session, "Latency");
				u.Commit();
		    }
		    return result;
	    }
    }
}