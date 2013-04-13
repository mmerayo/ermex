using System;
using System.Diagnostics;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Connectivity
{
	internal sealed class ConnectivityDetailsWritter : ICanWriteConnectivityDetails
	{
		private readonly IPersistRepository<ConnectivityDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;


		[Inject]
		public ConnectivityDetailsWritter(IPersistRepository<ConnectivityDetails> repository,
			IUnitOfWorkFactory factory,IComponentSettings settings)
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public void RemoveComponentDetails(Guid componentId)
		{
			using (var uow = _factory.Create())
			{
				_repository.Remove(uow, x=>x.ServerId== componentId);
				uow.Commit();
			}
		}

		public void RemoveLocalComponentDetails()
		{
			using (var uow = _factory.Create())
			{
				_repository.Remove(uow, x => x.ServerId == _settings.ComponentId);
				uow.Commit();
			}
		}

		public ConnectivityDetails CreateComponentConnectivityDetails(ushort port, bool asLocal = true)
		{
			ConnectivityDetails componentConnectivityDetails;
			using (var uow = _factory.Create())
			{
				componentConnectivityDetails = CreateComponentConnectivityDetails(uow, port, asLocal);
				uow.Commit();
			}
			return componentConnectivityDetails;
		}

		public ConnectivityDetails CreateComponentConnectivityDetails(IUnitOfWork unitOfWork, ushort port, bool asLocal = true)
		{
			var connectivityDetails = new ConnectivityDetails
			{
				ComponentOwner = _settings.ComponentId,
				ServerId = _settings.ComponentId,
				Ip = Networking.GetLocalhostIp(),
				Port = port,
				IsLocal = asLocal
			};
			
				_repository.Save(unitOfWork, connectivityDetails);
			
			Debug.Assert(connectivityDetails.Id!=0,"The id was not populated");
			return connectivityDetails;
		}
	}
}