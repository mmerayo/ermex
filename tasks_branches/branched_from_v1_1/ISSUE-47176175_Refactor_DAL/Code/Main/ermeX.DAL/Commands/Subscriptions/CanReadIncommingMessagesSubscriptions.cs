using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Subscriptions
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
				result = _repository.Where(uow, x => x.BizMessageFullTypeName == bizMessageType).ToList();
				uow.Commit();
			}
			return result;
		}

		public IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId)
		{
			IncomingMessageSuscription result;
			using (var uow = _factory.Create())
			{
				result = _repository.SingleOrDefault(uow, x => x.SuscriptionHandlerId == suscriptionHandlerId);
				uow.Commit();
			}

			return result;
		}

		public IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType)
		{
			IncomingMessageSuscription result;
			using (var uow = _factory.Create())
			{
				result = _repository.SingleOrDefault(uow, x => x.HandlerType == handlerType.FullName && x.BizMessageFullTypeName == messageType.FullName);
				uow.Commit();
			}

			return result;
		}

		public IEnumerable<IncomingMessageSuscription> FetchAll()
		{
			IEnumerable<IncomingMessageSuscription> result;
			using (var uow = _factory.Create())
			{
				result = _repository.FetchAll(uow).ToList();
				uow.Commit();
			}
			return result;
		}
	}
}