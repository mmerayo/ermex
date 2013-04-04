using System;
using System.Diagnostics;
using System.Net.Sockets;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Connectivity
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
				_repository.Remove(x=>x.ServerId== componentId);
				uow.Commit();
			}
		}

		public void RemoveLocalComponentDetails()
		{
			using (var uow = _factory.Create())
			{
				_repository.Remove(x => x.ServerId == _settings.ComponentId);
				uow.Commit();
			}
		}

		public ConnectivityDetails CreateComponentConnectivityDetails(ushort port, bool asLocal = true)
		{
			var connectivityDetails = new ConnectivityDetails
			{
				ComponentOwner = _settings.ComponentId,
				ServerId = _settings.ComponentId,
				Ip = Networking.GetLocalhostIp(),
				Port = port,
				IsLocal = asLocal
			};
			using (var uow = _factory.Create())
			{
				_repository.Save(connectivityDetails);
				uow.Commit();
			}
			Debug.Assert(connectivityDetails.Id!=0,"The id was not populated");
			return connectivityDetails;
		}
	}
}