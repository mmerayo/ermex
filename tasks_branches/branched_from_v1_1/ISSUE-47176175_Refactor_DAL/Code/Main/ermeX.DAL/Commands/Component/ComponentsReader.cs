using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Domain.Component;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Component
{
    internal sealed class ComponentsReader : ICanReadComponents
    {
	    private readonly IUnitOfWorkFactory _factory;
	    private readonly IComponentSettings _settings;
	    private IReadOnlyRepository<AppComponent> Repository { get; set; }

        [Inject]
        public ComponentsReader(IReadOnlyRepository<AppComponent> repository, IUnitOfWorkFactory factory,
			IComponentSettings settings)
        {
	        _factory = factory;
	        _settings = settings;
	        Repository = repository;
        }

	    public IEnumerable<AppComponent> FetchAll()
	    {
		    IQueryable<AppComponent> result;
		    using (var uow=_factory.Create())
		    {
			    result = Repository.FetchAll();
			    uow.Commit();
		    }
		    return result;
	    }

	    public IEnumerable<AppComponent> FetchOtherComponents()
        {
			IQueryable<AppComponent> result;
			using (var uow = _factory.Create())
			{
				result = Repository.Where(x=>x.ComponentId!=_settings.ComponentId);
				uow.Commit();
			}
			return result;
        }

	    public IEnumerable<AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false)
	    {
		    IQueryable<AppComponent> result;
		    using (var uow = _factory.Create())
		    {
			    result = Repository.Where(x => x.ComponentId != _settings.ComponentId
			                                   && !x.ExchangedDefinitions
			                                   && x.IsRunning == running);
			    uow.Commit();
		    }
		    return result;
	    }

	    public AppComponent Fetch(Guid componentId)
	    {
		    AppComponent result;
			using (var uow = _factory.Create())
			{
				result = Repository.SingleOrDefault(x=>x.ComponentId==componentId);
				uow.Commit();
			}
			return result;
        }

    }
}