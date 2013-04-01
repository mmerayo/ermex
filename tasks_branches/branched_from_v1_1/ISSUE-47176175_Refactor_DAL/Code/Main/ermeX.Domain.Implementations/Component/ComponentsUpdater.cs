using System;
using Ninject;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Component;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Component
{
    internal sealed class ComponentsUpdater : ICanWriteComponents
    {
         private IAppComponentDataSource Repository { get; set; }

        [Inject]
         public ComponentsUpdater(IAppComponentDataSource repository)
        {
            Repository = repository;
        }

        public bool ImportFromOtherComponent(AppComponent entity, ConnectivityDetails connectivityDetails)
        {
            //TODO: Simplify usage
            return Repository.SaveFromOtherComponent(entity, connectivityDetails);
        }

        public void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false)
        {
            //TODO: MOVE TO ANOTHER DECORATOR
            Repository.SetComponentRunningStatus(componentId,newStatus,exchangedDefinitions);
        }

	    public void Save(AppComponent component)
	    {
		    Repository.Save(component); //TODO: Issue-281: sEE USAGE AND REFACTOR
	    }
    }
}