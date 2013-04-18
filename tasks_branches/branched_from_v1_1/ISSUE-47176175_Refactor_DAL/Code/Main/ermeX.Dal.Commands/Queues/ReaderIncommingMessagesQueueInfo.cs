using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Queues;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Queues
{
	class ReaderIncommingQueue : IReadIncommingQueue
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ReaderIncommingQueue).FullName);
		private readonly IReadOnlyRepository<IncomingMessage> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public ReaderIncommingQueue(IReadOnlyRepository<IncomingMessage> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
        {
			Logger.Debug("cctor");
	        _repository = repository;
	        _factory = factory;
        }

		public IncomingMessage GetNextDispatchableItem(int maxLatency)
		{
			Logger.DebugFormat("GetNextDispatchableItem. maxLatency={0}",maxLatency);
			
			IncomingMessage result = null;
			using (var uow = _factory.Create())
			{
				IOrderedQueryable<IncomingMessage> incomingMessages = _repository
					.Where(uow,x => x.Status == Message.MessageStatus.ReceiverDispatching)
					.OrderBy(x => x.CreatedTimeUtc);
				foreach (var incomingMessage in incomingMessages)
				{
					var timeSpan = DateTime.UtcNow.Subtract(incomingMessage.CreatedTimeUtc);
					var milliseconds = timeSpan.TotalMilliseconds;
					if (milliseconds >= maxLatency)
					{
						result = incomingMessage;
						break;
					}
				}

				uow.Commit();
			}
			return result;
		}

		public IEnumerable<IncomingMessage> GetMessagesToDispatch()
		{
			Logger.Debug("GetMessagesToDispatch");
			IEnumerable<IncomingMessage> result;
			using (var uow = _factory.Create())
			{
				result = _repository
					.Where(uow,x => x.Status == Message.MessageStatus.ReceiverReceived)
					.OrderBy(x => x.CreatedTimeUtc).ToList();
				uow.Commit();
			}
			return result;
		}

		public IEnumerable<IncomingMessage> GetByStatus(params Message.MessageStatus[] status)
		{
			Logger.Debug("GetByStatus");

			IEnumerable<IncomingMessage> result;
			using (var uow = _factory.Create())
			{
				result = _repository
					.Where(uow, x => status.Contains(x.Status))
					.OrderBy(x => x.CreatedTimeUtc).ToList();
				uow.Commit();
			}
			return result;
		}

		public bool ContainsMessageFor(Guid messageId, Guid destinationComponent)
		{
			Logger.DebugFormat("ContainsMessageFor. messageId={0}, destinationComponent={1}",messageId,destinationComponent);

			if (messageId.IsEmpty() || destinationComponent.IsEmpty())
				throw new ArgumentException("the arguments cannot be empty");

			bool result;
			using (var uow = _factory.Create())
			{
				result =
					_repository.Any(uow, x => x.MessageId == messageId && x.SuscriptionHandlerId == destinationComponent);
				uow.Commit();
			}
			return result;
		}

		public IEnumerable<IncomingMessage> GetNonDistributedMessages()
		{
			Logger.Debug("GetNonDistributedMessages");

			IEnumerable<IncomingMessage> result;
			using (var uow = _factory.Create())
			{
				result = _repository.Where(uow, x => x.Status == Message.MessageStatus.ReceiverReceived && x.SuscriptionHandlerId == Guid.Empty).ToList();
				uow.Commit();
			}
			return result;
		}
	}
}