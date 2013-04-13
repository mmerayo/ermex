using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Component;
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
		    List<AppComponent> result;
		    using (var uow=_factory.Create())
		    {
			    result = Repository.FetchAll(uow).ToList();
			    uow.Commit();
		    }
		    return result;
	    }

	    public IEnumerable<AppComponent> FetchOtherComponents()
        {
			List<AppComponent> result;
			using (var uow = _factory.Create())
			{
				result = Repository.Where(uow,x=>x.ComponentId!=_settings.ComponentId).ToList();
				uow.Commit();
			}
			return result;
        }

	    public IEnumerable<AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false)
	    {
		    List<AppComponent> result;
		    using (var uow = _factory.Create())
		    {
			    result = Repository.Where(uow,x => x.ComponentId != _settings.ComponentId
			                                   && !x.ExchangedDefinitions
			                                   && x.IsRunning == running).ToList();
			    uow.Commit();
		    }
		    return result;
	    }

	    public AppComponent Fetch(Guid componentId)
	    {
		    AppComponent result;
			using (var uow = _factory.Create())
			{
				result = Repository.SingleOrDefault(uow,x=>x.ComponentId==componentId);
				uow.Commit();
			}
			return result;
        }

    }
}