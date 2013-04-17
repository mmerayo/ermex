using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Services;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Services
{
	class ServiceDetailsReader : ICanReadServiceDetails
	{
		private readonly IReadOnlyRepository<ServiceDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public ServiceDetailsReader(IReadOnlyRepository<ServiceDetails> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public ServiceDetails GetByOperationId(Guid operationId)
		{
			
			return GetByOperationId(_settings.ComponentId, operationId);
		}

		public ServiceDetails GetByOperationId(Guid publisher, Guid operationId)
		{
			ServiceDetails result;
			using (var uow = _factory.Create())
			{
				result = _repository.SingleOrDefault(uow, x => x.OperationIdentifier == operationId 
					&& x.Publisher==publisher);
				uow.Commit();
			}
			return result;
		}

		public IEnumerable<ServiceDetails> GetByInterfaceType(Type interfaceType)
		{
			return GetByInterfaceType(interfaceType.FullName);
		}

		public IEnumerable<ServiceDetails> GetByInterfaceType(string interfaceTypeFullName)
		{
			if (string.IsNullOrEmpty(interfaceTypeFullName)) throw new ArgumentNullException("interfaceTypeFullName");
			IEnumerable<ServiceDetails> result;
			using (var uow = _factory.Create())
			{
				result = _repository.Where(uow, x => x.ServiceInterfaceTypeName == interfaceTypeFullName).ToList();
				uow.Commit();
			}
			return result;
		}

		public IEnumerable<ServiceDetails> GetByMethodName(string interfaceTypeName, string methodName)
		{
			if (string.IsNullOrEmpty(interfaceTypeName)) throw new ArgumentNullException("interfaceTypeName");
			IEnumerable<ServiceDetails> result;
			using (var uow = _factory.Create())
			{
				result = _repository.Where(uow, x => x.ServiceInterfaceTypeName == interfaceTypeName
					&& x.ServiceImplementationMethodName == methodName).ToList();
				uow.Commit();
			}
			return result;
		}

		public ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent)
		{
			if (string.IsNullOrEmpty(interfaceTypeName)) throw new ArgumentNullException("interfaceTypeName");
			ServiceDetails result;
			using (var uow = _factory.Create())
			{
				result = _repository.SingleOrDefault(uow, x => x.ServiceInterfaceTypeName == interfaceTypeName
					&& x.ServiceImplementationMethodName == methodName && x.Publisher==publisherComponent);
				uow.Commit();
			}
			return result;
		}

		public IEnumerable<ServiceDetails> GetLocalCustomServices()
		{
			IEnumerable<ServiceDetails> result;
			using (var uow = _factory.Create())
			{
				result = _repository.Where(uow, x => x.Publisher == _settings.ComponentId
					&& x.IsSystemService == false).ToList();
				uow.Commit();
			}
			return result;
		}
	}
}