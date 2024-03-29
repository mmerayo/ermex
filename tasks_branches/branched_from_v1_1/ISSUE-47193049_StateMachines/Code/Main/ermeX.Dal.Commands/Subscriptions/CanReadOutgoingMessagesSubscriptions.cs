using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Subscriptions
{
	class CanReadOutgoingMessagesSubscriptions : ICanReadOutgoingMessagesSubscriptions
	{
		private readonly ILogger Logger;

		private readonly IReadOnlyRepository<OutgoingMessageSuscription> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public CanReadOutgoingMessagesSubscriptions(IReadOnlyRepository<OutgoingMessageSuscription> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			Logger = LogManager.GetLogger(typeof (CanReadOutgoingMessagesSubscriptions), settings.ComponentId,
			                              LogComponent.DataServices);
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
		}

		public IEnumerable<OutgoingMessageSuscription> GetByMessageType(string bizMessageType)
		{
			Logger.TraceFormat("GetByMessageType. bizMessageType={0} AppDomain={1} - Thread={2}", bizMessageType, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			IEnumerable<OutgoingMessageSuscription> result = null;

			_factory.ExecuteInUnitOfWork(true,
				uow => result = _repository.Where(uow, x => x.BizMessageFullTypeName == bizMessageType).ToList());
			return result;

		}

		public IEnumerable<OutgoingMessageSuscription> FetchAll()
		{
			Logger.TraceFormat("FetchAll.AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			IEnumerable<OutgoingMessageSuscription> result = null;
			_factory.ExecuteInUnitOfWork(true,uow => result = _repository.FetchAll(uow).ToList());
			return result;
		}
	}
}