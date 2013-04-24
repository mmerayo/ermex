using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Services;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
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
			Logger.TraceFormat("GetByMessageType. bizMessageType={0} AppDomain={1} - Thread={2}", bizMessageType, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			IEnumerable<IncomingMessageSuscription> result = null;
			_factory.ExecuteInUnitOfWork(true,
				uow => result = _repository.Where(uow, x => x.BizMessageFullTypeName == bizMessageType).ToList());
			return result;
		}

		public IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId)
		{
			Logger.TraceFormat("GetByHandlerId. suscriptionHandlerId={0} AppDomain={1} - Thread={2}", suscriptionHandlerId, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			IncomingMessageSuscription result = null;
			_factory.ExecuteInUnitOfWork(true,
				uow => result = _repository.SingleOrDefault(uow, x => x.SuscriptionHandlerId == suscriptionHandlerId));

			return result;
		}

		public IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType)
		{
			Logger.TraceFormat("GetByHandlerAndMessageType. handlerType={0}, messageType={1} AppDomain={2} - Thread={3}", handlerType, messageType, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			IncomingMessageSuscription result = null;
			_factory.ExecuteInUnitOfWork(true,
				uow =>
				result =
				_repository.SingleOrDefault(uow,
				                            x =>
				                            x.HandlerType == handlerType.FullName &&
				                            x.BizMessageFullTypeName == messageType.FullName));

			return result;
		}

		public IEnumerable<IncomingMessageSuscription> FetchAll()
		{
			Logger.TraceFormat("FetchAll.AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			IEnumerable<IncomingMessageSuscription> result = null;

			_factory.ExecuteInUnitOfWork(true, uow => result = _repository.FetchAll(uow).ToList());
			return result;
		}
	}
}