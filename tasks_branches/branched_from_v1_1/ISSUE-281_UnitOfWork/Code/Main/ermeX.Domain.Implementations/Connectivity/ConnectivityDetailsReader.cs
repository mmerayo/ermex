using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Connectivity
{
    internal class ConnectivityDetailsReader : ICanGetConnectivityDetails
    {
        private IConnectivityDetailsDataSource Repository { get; set; }

        [Inject]
        public ConnectivityDetailsReader(IConnectivityDetailsDataSource repository)
        {
            Repository = repository;
        }

        public ConnectivityDetails Fetch(Guid componentId)
        {
            return Repository.GetByComponentId(componentId);
        }
    }
}