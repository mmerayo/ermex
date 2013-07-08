using System;
using System.Diagnostics;
using System.Threading;

using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Connectivity
{
	internal sealed class ConnectivityDetailsWritter : ICanWriteConnectivityDetails
	{
		private readonly ILogger Logger;
		private readonly IPersistRepository<ConnectivityDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;


		[Inject]
		public ConnectivityDetailsWritter(IPersistRepository<ConnectivityDetails> repository,
			IUnitOfWorkFactory factory,IComponentSettings settings)
		{
			Logger = LogManager.GetLogger(typeof (ConnectivityDetailsWritter), settings.ComponentId, LogComponent.DataServices);
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public void RemoveComponentDetails(Guid componentId)
		{
			Logger.TraceFormat("RemoveComponentDetails. AppDomain={0} - Thread={1} -componentId={2}",AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, componentId);
			_factory.ExecuteInUnitOfWork(false,uow => _repository.Remove(uow, x => x.ServerId == componentId));
		}

		public void RemoveLocalComponentDetails()
		{
			Logger.TraceFormat("RemoveLocalComponentDetails. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Remove(uow, x => x.ServerId == _settings.ComponentId));
		}

		public ConnectivityDetails CreateComponentConnectivityDetails(ushort port, bool asLocal = true)
		{
			Logger.TraceFormat("CreateComponentConnectivityDetails.AppDomain={0} - Thread={1} - port={2} - asLocal={3}",AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, port, asLocal);
			ConnectivityDetails componentConnectivityDetails = null;
			_factory.ExecuteInUnitOfWork(false,
				uow => componentConnectivityDetails = CreateComponentConnectivityDetails(uow, port, asLocal));
			return componentConnectivityDetails;
		}

		public ConnectivityDetails CreateComponentConnectivityDetails(IUnitOfWork unitOfWork, ushort port, bool asLocal = true)
		{
			Logger.DebugFormat("CreateComponentConnectivityDetails.AppDomain={0} - Thread={1} - port={2}, asLocal={3}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId,port, asLocal);
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