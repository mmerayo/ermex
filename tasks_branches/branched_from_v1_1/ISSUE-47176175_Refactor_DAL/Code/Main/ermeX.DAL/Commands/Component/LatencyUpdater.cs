using System;
using Ninject;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Component;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class LatencyUpdater : ICanUpdateLatency
	{
		private readonly IUnitOfWorkFactory _factory;
		private IPersistRepository<AppComponent> Repository { get; set; }

		[Inject]
		public LatencyUpdater(IPersistRepository<AppComponent> repository, IUnitOfWorkFactory factory)
		{
			_factory = factory;
			Repository = repository;
		}

		public void RegisterComponentRequestLatency(Guid remoteComponentId, int requestMilliseconds)
		{
			using (var uow = _factory.Create())
			{
				var senderComponent = Repository.SingleOrDefault(uow.Session, x => x.ComponentId == remoteComponentId);
				if (senderComponent != null)
				{
					senderComponent.Latency = (senderComponent.Latency + requestMilliseconds)/2;
					Repository.Save(uow.Session, senderComponent);
				}
				uow.Commit();
			}
		}
	}
}