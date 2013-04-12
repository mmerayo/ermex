using System;
using NHibernate;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Commands.Connectivity;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Entities.Entities;
using ermeX.ConfigurationManagement.IoC;

namespace ermeX.DAL.Commands.Component
{

	//TODO: MOVE TO THE COMPONENTS UPDATER
	internal sealed class ComponentsRegistrator : IRegisterComponents
	{
		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<AppComponent> _componentsRepository;
		private readonly IPersistRepository<ConnectivityDetails> _connectivityRepository;
		private readonly IPersistRepository<ServiceDetails> _serviceDetailsRepository;
		private readonly IComponentSettings _settings;
		private readonly IStatusManager _statusManager;
		private readonly ICanWriteConnectivityDetails _connectivityDetailsWritter;

		[Inject]
		public ComponentsRegistrator(IUnitOfWorkFactory factory,
		                             IPersistRepository<AppComponent> componentsRepository,
		                             IPersistRepository<ConnectivityDetails> connectivityRepository,
		                             IPersistRepository<ServiceDetails> serviceDetailsRepository,
		                             IComponentSettings settings,
		                             IStatusManager statusManager,
			ICanWriteConnectivityDetails connectivityDetailsWritter)
		{
			_factory = factory;
			_componentsRepository = componentsRepository;
			_connectivityRepository = connectivityRepository;
			_serviceDetailsRepository = serviceDetailsRepository;
			_settings = settings;
			_statusManager = statusManager;
			_connectivityDetailsWritter = connectivityDetailsWritter;
		}

		public bool CreateRemoteComponent(Guid remoteComponentId, string ip, int port)
		{
			bool result;
			using (var uow = _factory.Create())
			{
				result = AddComponentFromRemote(uow.Session,remoteComponentId);
				AddConnectivityDetailsFromRemote(uow.Session,remoteComponentId, ip, port);
				RegisterSystemServices(uow.Session,remoteComponentId);

				uow.Commit();
			}

			return result;

		}

		public void CreateLocalComponent(ushort port)
		{
			using (var uow = _factory.Create())
			{
				CreateLocalAppComponent(uow.Session);

				//TODO: CREATE LOCAL INTERFACE THAT ACCEPTS UOWS
				((ConnectivityDetailsWritter)_connectivityDetailsWritter).CreateComponentConnectivityDetails(uow, port, true);

				RegisterSystemServices(uow.Session,_settings.ComponentId);
				uow.Commit();
			}
		}

		

		private void CreateLocalAppComponent(ISession session)
		{
			var appComponent = new AppComponent
				{
					ComponentOwner = _settings.ComponentId,
					ComponentId = _settings.ComponentId,
					IsRunning = _statusManager.CurrentStatus == ComponentStatus.Running,
					ExchangedDefinitions = true
				};
			_componentsRepository.Save(session, appComponent);
		}


		private bool AddComponentFromRemote(ISession session,Guid remoteComponentId)
		{

			var appComponent = _componentsRepository.SingleOrDefault(session, x => x.ComponentId == remoteComponentId);

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
			_componentsRepository.Save(session, appComponent);
			return isNew;
		}

		private void AddConnectivityDetailsFromRemote(ISession session,Guid remoteComponentId, string ip, int port)
		{
			ConnectivityDetails connectivityDetails =
				_connectivityRepository.SingleOrDefault(session, x => x.ServerId == remoteComponentId) ?? new ConnectivityDetails();
			connectivityDetails.ComponentOwner = _settings.ComponentId;
			connectivityDetails.Ip = ip;
			connectivityDetails.Port = port;
			connectivityDetails.ServerId = remoteComponentId;
			connectivityDetails.Version = DateTime.MinValue.Ticks + 1;

			_connectivityRepository.Save(session, connectivityDetails);

		}

		private void RegisterSystemServices(ISession session,Guid componentId)
		{
			//TODO: BY TYPE
			RegisterSystemServices(session,TypesHelper.GetTypeFromDomainByClassName("IHandshakeService"), componentId);
			RegisterSystemServices(session,TypesHelper.GetTypeFromDomainByClassName("IMessageSuscriptionsService"), componentId);
			RegisterSystemServices(session,TypesHelper.GetTypeFromDomainByClassName("IPublishedServicesDefinitionsService"), componentId);

		}


		private void RegisterSystemServices( ISession session,Type typeService, Guid remoteComponentId)
		{
			if (typeService == null) throw new ArgumentNullException("typeService");
			var serviceOperationAttributes = ServiceOperationAttribute.GetServiceNames(typeService);
			foreach (var name in serviceOperationAttributes)
				RegisterSystemService(session,remoteComponentId, name, typeService);
		}

		private void RegisterSystemService(ISession session,Guid componentId, string serviceImplementationMethodName, Type serviceInterfaceType)
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
			_serviceDetailsRepository.Save(session, serviceDetails);

		}
	}
}