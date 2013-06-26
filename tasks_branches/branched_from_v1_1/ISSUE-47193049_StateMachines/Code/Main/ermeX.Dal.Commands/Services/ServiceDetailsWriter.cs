using System;
using System.Linq.Expressions;
using System.Threading;

using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Services;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Services
{
	class ServiceDetailsWriter : ICanWriteServiceDetails
	{
		private static readonly ILogger Logger = LogManager.GetLogger(typeof(ServiceDetailsWriter).FullName);

		private readonly IPersistRepository<ServiceDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public ServiceDetailsWriter(IPersistRepository<ServiceDetails> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public void ImportFromOtherComponent(ServiceDetails svc)
		{
			Logger.TraceFormat("ImportFromOtherComponent, svc={0}-AppDomain={1} - Thread={2}", svc, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);

			if(svc.ComponentOwner==_settings.ComponentId)
				throw new InvalidOperationException("Cannot import one service from the same component");
			const string RemoteTypeImplementorValue = "<<REMOTE>>";
			svc.Id = 0;
			svc.ServiceImplementationTypeName = RemoteTypeImplementorValue;
			svc.ComponentOwner = _settings.ComponentId;
			//TODO: FIX LOGICAL EXPRESSIONHELPER FOR THIS ENTITY_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, svc));
			_factory.ExecuteInUnitOfWork(false, uow=> _repository.Save(uow,svc));
			//using (var uow = _factory.Create(false))
			//{
			//    Expression<Func<ServiceDetails, bool>> expression = x => x.OperationIdentifier == svc.OperationIdentifier && x.Publisher == svc.Publisher;
			//    if (_repository.Any(uow, expression))
			//    {
			//        ServiceDetails serviceDetails = _repository.Single(uow, expression);
			//        svc.Id = serviceDetails.Id;
			//        uow.Session.Merge(svc);
			//        uow.Flush();
			//    }
			//    else
			//    {
			//        svc.Id = 0;
			//    }
			//    _repository.Save(uow, svc);
			//    uow.Commit();
			//}
		}

		public void Save(ServiceDetails serviceDetails)
		{
			Logger.TraceFormat("Save. serviceDetails={0} AppDomain={1} - Thread={2}", serviceDetails, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);

			_factory.ExecuteInUnitOfWork(false,uow => _repository.Save(uow, serviceDetails));
		}
	}
}