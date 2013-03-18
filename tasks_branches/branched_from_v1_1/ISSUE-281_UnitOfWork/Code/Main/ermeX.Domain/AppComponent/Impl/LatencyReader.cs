using Ninject;
using ermeX.DAL.Interfaces;

namespace ermeX.Domain.AppComponent.Impl
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