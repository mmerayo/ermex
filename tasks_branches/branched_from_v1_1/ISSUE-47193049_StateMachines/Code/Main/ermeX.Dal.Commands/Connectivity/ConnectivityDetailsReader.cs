using System;
using System.Threading;

using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Connectivity
{
	internal class ConnectivityDetailsReader : ICanReadConnectivityDetails
	{
		private readonly ILogger _logger;
		private readonly IReadOnlyRepository<ConnectivityDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public ConnectivityDetailsReader(IReadOnlyRepository<ConnectivityDetails> repository,
		                                 IUnitOfWorkFactory factory,IComponentSettings settings)
		{
			_logger = LogManager.GetLogger<ConnectivityDetailsReader>(settings.ComponentId,LogComponent.DataServices);
			_logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			
			_repository = repository;
			_factory = factory;
		}

		public ConnectivityDetails Fetch(Guid componentId)
		{
			_logger.TraceFormat("Fetch. AppDomain={0} - Thread={1} - componentId={2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, componentId);
			ConnectivityDetails result=null;
			_factory.ExecuteInUnitOfWork(true,uow => result = _repository.SingleOrDefault(uow, x => x.ServerId == componentId));
			return result;
		}
	}
}