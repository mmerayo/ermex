﻿using System;
using System.Linq.Expressions;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Services;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Services
{
	class ServiceDetailsWriter : ICanWriteServiceDetails
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ServiceDetailsWriter).FullName);

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
			Logger.DebugFormat("ImportFromOtherComponent, svc={0}",svc);

			if(svc.ComponentOwner==_settings.ComponentId)
				throw new InvalidOperationException("Cannot import one service from the same component");
			const string RemoteTypeImplementorValue = "<<REMOTE>>";
			svc.ServiceImplementationTypeName = RemoteTypeImplementorValue;
			svc.ComponentOwner = _settings.ComponentId;

			using (var uow = _factory.Create())
			{
				Expression<Func<ServiceDetails, bool>> expression = x => x.OperationIdentifier == svc.OperationIdentifier && x.Publisher == svc.Publisher;
				if (_repository.Any(uow, expression))
				{
					ServiceDetails serviceDetails = _repository.Single(uow, expression);
					svc.Id = serviceDetails.Id;
					uow.Session.Merge(svc);
					uow.Flush();
				}
				else
				{
					svc.Id = 0;
				}
				_repository.Save(uow, svc);
				uow.Commit();
			}
		}

		public void Save(ServiceDetails serviceDetails)
		{
			Logger.DebugFormat("Save. serviceDetails={0}",serviceDetails);
			using (var uow = _factory.Create())
			{
				_repository.Save(uow, serviceDetails);
				uow.Commit();
			}
		}
	}
}