using System;
using Common.Logging;
using Ninject;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
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
			Logger.Debug("cctor");
			_repository = repository;
			_factory = factory;
		}

		public ConnectivityDetails Fetch(Guid componentId)
		{
			Logger.DebugFormat("Fetch.componentId={0}", componentId);
			ConnectivityDetails result;
			using (var uow = _factory.Create())
			{
				result = _repository.SingleOrDefault(uow, x => x.ServerId == componentId);
				uow.Commit();
			}
			return result;
		}
	}
}