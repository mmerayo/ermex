using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Component;

namespace ermeX.Domain.Implementations.Component
{
    internal sealed class LatencyReader : ICanReadLatency
    {
        private IAppComponentDataSource Repository { get; set; }

        [Inject]
        public LatencyReader(IAppComponentDataSource repository)
        {
            Repository = repository;
        }

        public int GetMaxLatency()
        {
            return Repository.GetMaxLatency();//TODO: THE CALCULATION TO BE DONE HERE
        }
    }
}