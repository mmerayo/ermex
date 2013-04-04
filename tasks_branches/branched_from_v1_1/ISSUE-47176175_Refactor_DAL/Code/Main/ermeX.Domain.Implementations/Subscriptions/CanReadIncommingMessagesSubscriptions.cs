using System;
using System.Collections.Generic;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Subscriptions
{
	class CanReadIncommingMessagesSubscriptions : ICanReadIncommingMessagesSubscriptions
	{
		private readonly IReadOnlyRepository<IncomingMessageSuscription> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public CanReadIncommingMessagesSubscriptions(IReadOnlyRepository<IncomingMessageSuscription> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public IEnumerable<IncomingMessageSuscription> GetByMessageType(string bizMessageType)
		{
			IEnumerable<IncomingMessageSuscription> result;
			using (var uow = _factory.Create())
			{
				result=_repository.Where(x => x.BizMessageFullTypeName == bizMessageType);
				uow.Commit();
			}
			return result;
		}

		public IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId)
		{
			IncomingMessageSuscription result;
			using (var uow = _factory.Create())
			{
				result = _repository.SingleOrDefault(x => x.SuscriptionHandlerId == suscriptionHandlerId);
				uow.Commit();
			}

			return result;
		}

		public IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType)
		{
			IncomingMessageSuscription result;
			using (var uow = _factory.Create())
			{
				result = _repository.SingleOrDefault(x => x.HandlerType == handlerType.FullName && x.BizMessageFullTypeName==messageType.FullName);
				uow.Commit();
			}

			return result;
		}

		public IEnumerable<IncomingMessageSuscription> FetchAll()
		{
			IEnumerable<IncomingMessageSuscription> result;
			using (var uow = _factory.Create())
			{
				result = _repository.FetchAll();
				uow.Commit();
			}
			return result;
		}
	}
}