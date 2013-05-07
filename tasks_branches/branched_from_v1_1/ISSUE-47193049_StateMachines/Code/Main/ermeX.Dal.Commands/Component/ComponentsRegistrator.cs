using System;
using System.Threading;
using Common.Logging;
using NHibernate;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Commands.Connectivity;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.ConfigurationManagement.IoC;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Component
{

	//TODO: MOVE TO THE COMPONENTS UPDATER
	internal sealed class ComponentsRegistrator : IRegisterComponents
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ComponentsRegistrator).FullName);
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
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
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
			Logger.TraceFormat("CreateRemoteComponent. AppDomain={0} - Thread={1} - remoteComponentId={2}, ip={3}, port={4}",
			                   AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, remoteComponentId, ip, port);
			bool result=false;
			_factory.ExecuteInUnitOfWork(false, uow =>
				{
					result = AddComponentFromRemote(uow, remoteComponentId);
					AddConnectivityDetailsFromRemote(uow, remoteComponentId, ip, port);
					RegisterSystemServices(uow, remoteComponentId);
				});
			
			return result;

		}

		public void CreateLocalComponent(ushort port)
		{
			Logger.TraceFormat("CreateLocalComponent. AppDomain={0} - Thread={1} - port={2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, port);
			_factory.ExecuteInUnitOfWork(false, (uow) =>
					CreateLocalComponent(uow,port)
				);
		}

		private void CreateLocalComponent(IUnitOfWork uow,ushort port)
		{
			CreateLocalAppComponent(uow);
			((ConnectivityDetailsWritter) _connectivityDetailsWritter).CreateComponentConnectivityDetails(uow, port, true);

			RegisterSystemServices(uow, _settings.ComponentId);
		}

		private void CreateLocalAppComponent(IUnitOfWork uow)
		{
			var appComponent = new AppComponent
				{
					ComponentOwner = _settings.ComponentId,
					ComponentId = _settings.ComponentId,
					IsRunning = _statusManager.CurrentStatus == ComponentStatus.Running,
					ExchangedDefinitions = true
				};
			_componentsRepository.Save(uow, appComponent);
		}


		private bool AddComponentFromRemote(IUnitOfWork uow,Guid remoteComponentId)
		{

			var appComponent = _componentsRepository.SingleOrDefault(uow, x => x.ComponentId == remoteComponentId);

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
			_componentsRepository.Save(uow, appComponent);
			return isNew;
		}

		private void AddConnectivityDetailsFromRemote(IUnitOfWork uow,Guid remoteComponentId, string ip, int port)
		{
			ConnectivityDetails connectivityDetails =
				_connectivityRepository.SingleOrDefault(uow, x => x.ServerId == remoteComponentId) ?? new ConnectivityDetails();
			connectivityDetails.ComponentOwner = _settings.ComponentId;
			connectivityDetails.Ip = ip;
			connectivityDetails.Port = port;
			connectivityDetails.ServerId = remoteComponentId;
			connectivityDetails.Version = DateTime.MinValue.Ticks + 1;

			_connectivityRepository.Save(uow, connectivityDetails);

		}

		private void RegisterSystemServices(IUnitOfWork uow,Guid componentId)
		{
			//TODO: BY TYPE
			RegisterSystemServices(uow,TypesHelper.GetTypeFromDomainByClassName("IHandshakeService"), componentId);
			RegisterSystemServices(uow,TypesHelper.GetTypeFromDomainByClassName("IMessageSuscriptionsService"), componentId);
			RegisterSystemServices(uow,TypesHelper.GetTypeFromDomainByClassName("IPublishedServicesDefinitionsService"), componentId);

		}


		private void RegisterSystemServices(IUnitOfWork uow, Type typeService, Guid remoteComponentId)
		{
			if (typeService == null) throw new ArgumentNullException("typeService");
			var serviceOperationAttributes = ServiceOperationAttribute.GetServiceNames(typeService);
			foreach (var name in serviceOperationAttributes)
				RegisterSystemService(uow,remoteComponentId, name, typeService);
		}

		private void RegisterSystemService(IUnitOfWork uow,Guid componentId, string serviceImplementationMethodName, Type serviceInterfaceType)
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
			_serviceDetailsRepository.Save(uow, serviceDetails);

		}
	}
}