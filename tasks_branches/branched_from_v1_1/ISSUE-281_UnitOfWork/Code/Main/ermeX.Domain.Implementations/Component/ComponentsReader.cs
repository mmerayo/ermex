using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Component;

namespace ermeX.Domain.Implementations.Component
{
    internal sealed class ComponentsReader : ICanReadComponents
    {
        private IAppComponentDataSource Repository { get; set; }

        [Inject]
        public ComponentsReader(IAppComponentDataSource repository)
        {
            Repository = repository;
        }

        #region ICanReadComponents Members

        public IList<Entities.Entities.AppComponent> FetchOtherComponents()
        {
            return Repository.GetOthers();
        }

        public IList<Entities.Entities.AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false)
        {
            return Repository.GetOtherComponentsWhereDefinitionsNotExchanged(running).ToList(); //TODO: LOGIC OUT OF REPOSITORY GENERIC CONTRACT TO BE HERE
        }

        Entities.Entities.AppComponent ICanReadComponents.Fetch(Guid componentId)
        {
            return Repository.GetByComponentId(componentId);
        }

        #endregion
    }
}