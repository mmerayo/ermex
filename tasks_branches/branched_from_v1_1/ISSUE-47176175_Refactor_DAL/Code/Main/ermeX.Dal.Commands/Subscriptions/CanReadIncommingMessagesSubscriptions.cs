using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Services;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Subscriptions
{
	class CanReadIncommingMessagesSubscriptions : ICanReadIncommingMessagesSubscriptions
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CanReadIncommingMessagesSubscriptions).FullName);
		private readonly IReadOnlyRepository<IncomingMessageSuscription> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public CanReadIncommingMessagesSubscriptions(IReadOnlyRepository<IncomingMessageSuscription> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public IEnumerable<IncomingMessageSuscription> GetByMessageType(string bizMessageType)
		{
			Logger.DebugFormat("GetByMessageType. bizMessageType={0}", bizMessageType);
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
			Logger.DebugFormat("GetByHandlerId. suscriptionHandlerId={0}", suscriptionHandlerId);
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
			Logger.DebugFormat("GetByHandlerAndMessageType. handlerType={0}, messageType={1}", handlerType,messageType);
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
			Logger.DebugFormat("FetchAll");
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