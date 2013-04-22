﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Services;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Services
{
	class ServiceDetailsReader : ICanReadServiceDetails
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceDetailsReader).FullName);
		private readonly IReadOnlyRepository<ServiceDetails> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public ServiceDetailsReader(IReadOnlyRepository<ServiceDetails> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
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
			Logger.DebugFormat("GetByOperationId. publisher={0}, operationId={1}",publisher,operationId);

			ServiceDetails result = null;

			_factory.ExecuteInUnitOfWork(
				uow => result = _repository.SingleOrDefault(uow, x => x.OperationIdentifier == operationId
				                                                      && x.Publisher == publisher));
			return result;
		}

		public IEnumerable<ServiceDetails> GetByInterfaceType(Type interfaceType)
		{
			return GetByInterfaceType(interfaceType.FullName);
		}

		public IEnumerable<ServiceDetails> GetByInterfaceType(string interfaceTypeFullName)
		{
			Logger.DebugFormat("GetByInterfaceType. interfaceTypeFullName={0}", interfaceTypeFullName);

			if (string.IsNullOrEmpty(interfaceTypeFullName)) throw new ArgumentNullException("interfaceTypeFullName");
			IEnumerable<ServiceDetails> result = null;
			_factory.ExecuteInUnitOfWork(
				uow => result = _repository.Where(uow, x => x.ServiceInterfaceTypeName == interfaceTypeFullName).ToList());
			return result;
		}

		public IEnumerable<ServiceDetails> GetByMethodName(string interfaceTypeName, string methodName)
		{
			Logger.DebugFormat("GetByMethodName. interfaceTypeName={0}, methodName={1}", interfaceTypeName, methodName);
			if (string.IsNullOrEmpty(interfaceTypeName)) throw new ArgumentNullException("interfaceTypeName");
			IEnumerable<ServiceDetails> result = null;

			_factory.ExecuteInUnitOfWork(
				uow => result = _repository.Where(uow, x => x.ServiceInterfaceTypeName == interfaceTypeName
				                                            && x.ServiceImplementationMethodName == methodName).ToList());
			
			return result;
		}

		public ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent)
		{
			Logger.DebugFormat("GetByMethodName. interfaceTypeName={0}, methodName={1}, publisherComponent={2}", interfaceTypeName, methodName,publisherComponent);
			if (string.IsNullOrEmpty(interfaceTypeName)) throw new ArgumentNullException("interfaceTypeName");
			ServiceDetails result = null;
			_factory.ExecuteInUnitOfWork(
				uow => result = _repository.SingleOrDefault(uow, x => x.ServiceInterfaceTypeName == interfaceTypeName
				                                                      && x.ServiceImplementationMethodName == methodName &&
				                                                      x.Publisher == publisherComponent));
			
			return result;
		}

		public IEnumerable<ServiceDetails> GetLocalCustomServices()
		{
			Logger.Debug("GetLocalCustomServices");
			IEnumerable<ServiceDetails> result = null;
			_factory.ExecuteInUnitOfWork(uow => result = _repository.Where(uow, x => x.Publisher == _settings.ComponentId
			                                                                         && x.IsSystemService == false).ToList());
			return result;
		}
	}
}