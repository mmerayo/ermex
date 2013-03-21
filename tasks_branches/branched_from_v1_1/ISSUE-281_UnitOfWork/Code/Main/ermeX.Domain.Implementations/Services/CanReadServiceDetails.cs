using System;
using System.Collections.Generic;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Services;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Services
{
	class CanReadServiceDetails : ICanReadServiceDetails
	{
		private readonly IServiceDetailsDataSource _repository;
		[Inject]
		public CanReadServiceDetails(IServiceDetailsDataSource repository)
		{
			_repository = repository;
		}

		public ServiceDetails GetByOperationId(Guid operationId)
		{
			return _repository.GetByOperationId(operationId);//TODO: MOVE LOGIC HERE
		}

		public ServiceDetails GetByOperationId(Guid publisher, Guid operationId)
		{
			return _repository.GetByOperationId(publisher, operationId);//TODO: MOVE LOGIC HERE
		}

		public IList<ServiceDetails> GetByInterfaceType(Type interfaceType)
		{
			return _repository.GetByInterfaceType(interfaceType);//TODO: MOVE LOGIC HERE
		}

		public IList<ServiceDetails> GetByInterfaceType(string interfaceTypeFullName)
		{
			return _repository.GetByInterfaceType(interfaceTypeFullName);//TODO: MOVE LOGIC HERE
		}

		public IList<ServiceDetails> GetByMethodName(string interfaceTypeName, string methodName)
		{
			return _repository.GetByMethodName(interfaceTypeName,methodName);//TODO: MOVE LOGIC HERE
		}

		public ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent)
		{
			return _repository.GetByMethodName(interfaceTypeName, methodName,publisherComponent);//TODO: MOVE LOGIC HERE
		}

		public IList<ServiceDetails> GetLocalCustomServices()
		{
			return _repository.GetLocalCustomServices(); //TODO: logic here
		}
	}
}