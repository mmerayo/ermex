using System;
using System.Threading;

using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class LatencyUpdater : ICanUpdateLatency
	{
		private readonly ILogger _logger;

		private readonly IUnitOfWorkFactory _factory;
		private IPersistRepository<AppComponent> Repository { get; set; }

		[Inject]
		public LatencyUpdater(IPersistRepository<AppComponent> repository, IUnitOfWorkFactory factory,IComponentSettings settings)
		{
			_logger = LogManager.GetLogger<LatencyUpdater>(settings.ComponentId, LogComponent.DataServices);
			_logger.TraceFormat("cctor. Thread={0}", Thread.CurrentThread.ManagedThreadId);
			_factory = factory;
			Repository = repository;
		}

		public void RegisterComponentRequestLatency(Guid remoteComponentId, int requestMilliseconds)
		{
			_logger.TraceFormat("RegisterComponentRequestLatency. AppDomain:{0} - threadId={1} - remoteComponentId:{2}, requestMilliseconds:{3}",
				AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId,
			                   remoteComponentId, requestMilliseconds);
			var senderComponent = Repository.SingleOrDefault(x => x.ComponentId == remoteComponentId);

			if (senderComponent != null)
			{
				senderComponent.Latency = (senderComponent.Latency + requestMilliseconds) / 2;

				_factory.ExecuteInUnitOfWork(false,uow =>
					{
						Repository.Save(uow, senderComponent);
					});
			}

		}
	}
}