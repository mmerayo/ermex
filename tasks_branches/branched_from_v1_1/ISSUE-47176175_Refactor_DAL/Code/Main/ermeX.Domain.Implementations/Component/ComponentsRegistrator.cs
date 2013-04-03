using System;
using NHibernate;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Component;
using ermeX.Entities.Entities;
using ermeX.ConfigurationManagement.IoC;

namespace ermeX.Domain.Implementations.Component
{
	internal sealed class ComponentsRegistrator : IRegisterComponents
	{
		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<AppComponent> _componentsRepository;
		private readonly IPersistRepository<ConnectivityDetails> _connectivityRepository;
		private readonly IPersistRepository<ServiceDetails> _serviceDetailsRepository;
		private readonly IComponentSettings _settings;
		private readonly IStatusManager _statusManager;

		[Inject]
		public ComponentsRegistrator(IUnitOfWorkFactory factory,
		                             IPersistRepository<AppComponent> componentsRepository,
		                             IPersistRepository<ConnectivityDetails> connectivityRepository,
		                             IPersistRepository<ServiceDetails> serviceDetailsRepository,
		                             IComponentSettings settings,
		                             IStatusManager statusManager)
		{
			_factory = factory;
			_componentsRepository = componentsRepository;
			_connectivityRepository = connectivityRepository;
			_serviceDetailsRepository = serviceDetailsRepository;
			_settings = settings;
			_statusManager = statusManager;
		}

		public bool CreateRemoteComponent(Guid remoteComponentId, string ip, int port)
		{
			bool result;
			using (var uow = _factory.Create())
			{
				result = AddComponentFromRemote(remoteComponentId);
				AddConnectivityDetailsFromRemote(remoteComponentId, ip, port);
				RegisterSystemServices(remoteComponentId);

				uow.Commit();
			}

			return result;

		}

		public void CreateLocalComponent(int port)
		{
			using (var uow = _factory.Create())
			{
				CreateLocalAppComponent();

				CreateLocalConnectivityDetails(port);

				RegisterSystemServices(_settings.ComponentId);
				uow.Commit();
			}
		}

		private void CreateLocalConnectivityDetails(int port)
		{
			var connectivityDetails = new ConnectivityDetails
				{
					ComponentOwner = _settings.ComponentId,
					ServerId = _settings.ComponentId,
					Ip = Networking.GetLocalhostIp(),
					Port = port
				};
			_connectivityRepository.Save(connectivityDetails);
		}

		private void CreateLocalAppComponent()
		{
			var appComponent = new AppComponent
				{
					ComponentOwner = _settings.ComponentId,
					ComponentId = _settings.ComponentId,
					IsRunning = _statusManager.CurrentStatus == ComponentStatus.Running,
					ExchangedDefinitions = true
				};
			_componentsRepository.Save(appComponent);
		}


		private bool AddComponentFromRemote(Guid remoteComponentId)
		{

			var appComponent = _componentsRepository.SingleOrDefault(x => x.ComponentId == remoteComponentId);

			bool isNew = false;
			if (appComponent == null)
			{
				appComponent = new AppComponent
					{
						ComponentOwner = _settings.ComponentId,
						ComponentId = remoteComponentId,
						IsRunning = false,
						ExchangedDefinitions = false
					};
				isNew = true;
			}

			appComponent.ExchangedDefinitions = false;
			_componentsRepository.Save(appComponent);
			return isNew;
		}

		private void AddConnectivityDetailsFromRemote(Guid remoteComponentId, string ip, int port)
		{
			ConnectivityDetails connectivityDetails =
				_connectivityRepository.SingleOrDefault(x => x.ServerId == remoteComponentId) ?? new ConnectivityDetails();
			connectivityDetails.ComponentOwner = _settings.ComponentId;
			connectivityDetails.Ip = ip;
			connectivityDetails.Port = port;
			connectivityDetails.ServerId = remoteComponentId;
			connectivityDetails.Version = DateTime.MinValue.Ticks + 1;

			_connectivityRepository.Save(connectivityDetails);

		}

		private void RegisterSystemServices(Guid componentId)
		{
			//TODO: BY TYPE
			RegisterSystemServices(TypesHelper.GetTypeFromDomainByClassName("IHandshakeService"), componentId);
			RegisterSystemServices(TypesHelper.GetTypeFromDomainByClassName("IMessageSuscriptionsService"), componentId);
			RegisterSystemServices(TypesHelper.GetTypeFromDomainByClassName("IPublishedServicesDefinitionsService"), componentId);

		}


		private void RegisterSystemServices(Type typeService, Guid remoteComponentId)
		{
			if (typeService == null) throw new ArgumentNullException("typeService");
			var serviceOperationAttributes = ServiceOperationAttribute.GetServiceNames(typeService);
			foreach (var name in serviceOperationAttributes)
				RegisterSystemService(remoteComponentId, name, typeService);
		}

		private void RegisterSystemService(Guid componentId, string serviceImplementationMethodName, Type serviceInterfaceType)
		{
			if (serviceInterfaceType == null) throw new ArgumentNullException("serviceInterfaceType");
			var isLocal = _settings.ComponentId == componentId;
			const string remoteServiceHint = "<<REMOTE>>";
			var serviceDetails = new ServiceDetails
				//TODO: this is repeated in other points locate them and refactor
				{
					ComponentOwner = _settings.ComponentId,
					OperationIdentifier =
						ServiceOperationAttribute.GetOperationIdentifier(
							serviceInterfaceType,
							serviceImplementationMethodName),
					//TODO: get FROM BASE CLASS
					Publisher = componentId,
					ServiceImplementationMethodName = serviceImplementationMethodName,
					ServiceImplementationTypeName =
						isLocal ? IoCManager.Kernel.Get(serviceInterfaceType).GetType().FullName : remoteServiceHint,
					ServiceInterfaceTypeName = serviceInterfaceType.FullName,
					IsSystemService = true,
					Version = DateTime.MinValue.Ticks + 1 //to force to update from node
				};
			_serviceDetailsRepository.Save(serviceDetails);

		}
	}
}