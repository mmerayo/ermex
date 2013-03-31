using System;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Component;

namespace ermeX.Domain.Implementations.Component
{
    internal sealed class LatencyUpdater : ICanUpdateLatency
    {
        private IAppComponentDataSource Repository { get; set; }

        [Inject]
        public LatencyUpdater(IAppComponentDataSource repository)
        {
            Repository = repository;
        }

        public void RegisterComponentRequestLatency(Guid componentId, int requestMilliseconds)
        {
            Repository.UpdateRemoteComponentLatency(componentId,requestMilliseconds);//TODO: THE PAYLOAD TO BE DONE HERE
        }
    }
}