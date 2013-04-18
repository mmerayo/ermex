using System;
using Common.Logging;
using Ninject;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Component;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class LatencyUpdater : ICanUpdateLatency
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(LatencyUpdater).FullName);

		private readonly IUnitOfWorkFactory _factory;
		private IPersistRepository<AppComponent> Repository { get; set; }

		[Inject]
		public LatencyUpdater(IPersistRepository<AppComponent> repository, IUnitOfWorkFactory factory)
		{
			Logger.Debug("cctor");
			_factory = factory;
			Repository = repository;
		}

		public void RegisterComponentRequestLatency(Guid remoteComponentId, int requestMilliseconds)
		{
			Logger.DebugFormat("RegisterComponentRequestLatency. remoteComponentId:{0}, requestMilliseconds:{1}",remoteComponentId,requestMilliseconds);
			using (var uow = _factory.Create())
			{
				var senderComponent = Repository.SingleOrDefault(uow, x => x.ComponentId == remoteComponentId);
				if (senderComponent != null)
				{
					senderComponent.Latency = (senderComponent.Latency + requestMilliseconds)/2;
					Repository.Save(uow, senderComponent);
				}
				uow.Commit();
			}
		}
	}
}