using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Connectivity.Impl
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
            Repository.GetByComponentId(componentId);
        }
    }
}