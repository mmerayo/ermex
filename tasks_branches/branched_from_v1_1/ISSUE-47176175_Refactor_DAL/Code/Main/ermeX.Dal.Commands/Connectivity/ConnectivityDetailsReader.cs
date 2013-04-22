using System;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Connectivity
{
	internal class ConnectivityDetailsReader : ICanReadConnectivityDetails
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ConnectivityDetailsReader).FullName);
		private readonly IReadOnlyRepository<ConnectivityDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public ConnectivityDetailsReader(IReadOnlyRepository<ConnectivityDetails> repository,
		                                 IUnitOfWorkFactory factory)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
		}

		public ConnectivityDetails Fetch(Guid componentId)
		{
			Logger.DebugFormat("Fetch.componentId={0}", componentId);
			ConnectivityDetails result=null;
			_factory.ExecuteInUnitOfWork(uow => result = _repository.SingleOrDefault(uow, x => x.ServerId == componentId));
			return result;
		}
	}
}