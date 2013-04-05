using System;
using System.Linq.Expressions;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Domain.Services;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Services
{
	class ServiceDetailsWriter : ICanWriteServiceDetails
	{
		private readonly IPersistRepository<ServiceDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public ServiceDetailsWriter(IPersistRepository<ServiceDetails> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public void ImportFromOtherComponent(ServiceDetails svc)
		{
			if(svc.ComponentOwner==_settings.ComponentId)
				throw new InvalidOperationException("Cannot import one service from the same component");
			const string RemoteTypeImplementorValue = "<<REMOTE>>";
			svc.ServiceImplementationTypeName = RemoteTypeImplementorValue;
			svc.ComponentOwner = _settings.ComponentId;

			using (var uow = _factory.Create())
			{
				Expression<Func<ServiceDetails, bool>> expression = x => x.OperationIdentifier == svc.OperationIdentifier && x.Publisher == svc.Publisher;
				if (_repository.Any(expression))
				{
					ServiceDetails serviceDetails = _repository.Single(expression);
					svc.Id = serviceDetails.Id;
					uow.Session.Evict(serviceDetails);
				}
				else
				{
					svc.Id = 0;
				}
				_repository.Save(svc);
				uow.Commit();
			}
		}

		public void Save(ServiceDetails serviceDetails)
		{
			using (var uow = _factory.Create())
			{
				_repository.Save(serviceDetails);
				uow.Commit();
			}
		}
	}
}