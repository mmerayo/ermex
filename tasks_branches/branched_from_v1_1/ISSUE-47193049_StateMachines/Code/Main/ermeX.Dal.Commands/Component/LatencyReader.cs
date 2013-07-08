using System;
using System.Threading;

using Ninject;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class LatencyReader : ICanReadLatency
	{
		private static readonly ILogger Logger = LogManager.GetLogger(typeof (LatencyReader),Guid.Empty,LogComponent.DataServices);
		private readonly IUnitOfWorkFactory _factory;
		private IReadOnlyRepository<AppComponent> Repository { get; set; }

		[Inject]
		public LatencyReader(IReadOnlyRepository<AppComponent> repository, IUnitOfWorkFactory factory)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_factory = factory;
			Repository = repository;
		}

		public int GetMaxLatency()
		{
			Logger.TraceFormat("GetMaxLatency. AppDomain={0} - Thread={1} ", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			Logger.Debug("GetMaxLatency");
			int result = 0;
			_factory.ExecuteInUnitOfWork(true, uow => result = Repository.GetMax<int>(uow, "Latency"));
			
			return result;
		}
	}
}