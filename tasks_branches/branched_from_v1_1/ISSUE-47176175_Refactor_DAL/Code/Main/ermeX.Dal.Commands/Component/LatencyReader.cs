using Common.Logging;
using Ninject;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Component;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class LatencyReader : ICanReadLatency
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (LatencyReader).FullName);
		private readonly IUnitOfWorkFactory _factory;
		private IReadOnlyRepository<AppComponent> Repository { get; set; }

		[Inject]
		public LatencyReader(IReadOnlyRepository<AppComponent> repository, IUnitOfWorkFactory factory)
		{
			Logger.Debug("cctor");
			_factory = factory;
			Repository = repository;
		}

		public int GetMaxLatency()
		{
			Logger.Debug("GetMaxLatency");
			int result = 0;
			using (var u = _factory.Create())
			{
				result = Repository.GetMax<int>(u, "Latency");
				u.Commit();
			}
			return result;
		}
	}
}