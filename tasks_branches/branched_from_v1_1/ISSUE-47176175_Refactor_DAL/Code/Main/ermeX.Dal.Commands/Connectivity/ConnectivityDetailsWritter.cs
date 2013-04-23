using System;
using System.Diagnostics;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Connectivity
{
	internal sealed class ConnectivityDetailsWritter : ICanWriteConnectivityDetails
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ConnectivityDetailsWritter).FullName);
		private readonly IPersistRepository<ConnectivityDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;


		[Inject]
		public ConnectivityDetailsWritter(IPersistRepository<ConnectivityDetails> repository,
			IUnitOfWorkFactory factory,IComponentSettings settings)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public void RemoveComponentDetails(Guid componentId)
		{
			Logger.DebugFormat("RemoveComponentDetails. componentId={0}", componentId);
			_factory.ExecuteInUnitOfWork(false,uow => _repository.Remove(uow, x => x.ServerId == componentId));
		}

		public void RemoveLocalComponentDetails()
		{
			Logger.Debug("RemoveLocalComponentDetails");
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Remove(uow, x => x.ServerId == _settings.ComponentId));
		}

		public ConnectivityDetails CreateComponentConnectivityDetails(ushort port, bool asLocal = true)
		{
			Logger.DebugFormat("CreateComponentConnectivityDetails. port={0} - asLocal={1}", port,asLocal);
			ConnectivityDetails componentConnectivityDetails = null;
			_factory.ExecuteInUnitOfWork(false,
				uow => componentConnectivityDetails = CreateComponentConnectivityDetails(uow, port, asLocal));
			return componentConnectivityDetails;
		}

		public ConnectivityDetails CreateComponentConnectivityDetails(IUnitOfWork unitOfWork, ushort port, bool asLocal = true)
		{
			Logger.DebugFormat("CreateComponentConnectivityDetails. port={0}, asLocal={1}", port, asLocal);
			var connectivityDetails = new ConnectivityDetails
				{
					ComponentOwner = _settings.ComponentId,
					ServerId = _settings.ComponentId,
					Ip = Networking.GetLocalhostIp(),
					Port = port,
					IsLocal = asLocal
				};
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(unitOfWork, connectivityDetails));

			Debug.Assert(connectivityDetails.Id != 0, "The id was not populated");
			return connectivityDetails;
		}
	}
}