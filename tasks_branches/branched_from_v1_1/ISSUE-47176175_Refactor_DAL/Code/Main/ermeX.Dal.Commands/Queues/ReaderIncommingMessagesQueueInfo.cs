using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
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
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
	        _repository = repository;
	        _factory = factory;
        }

		public IncomingMessage GetNextDispatchableItem(int maxLatency)
		{
			Logger.DebugFormat("GetNextDispatchableItem. maxLatency={0}",maxLatency);
			
			IncomingMessage result = null;
			_factory.ExecuteInUnitOfWork(uow =>
				{
					IOrderedQueryable<IncomingMessage> incomingMessages = _repository
										.Where(uow, x => x.Status == Message.MessageStatus.ReceiverDispatching)
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
				});
			return result;
		}

		public IEnumerable<IncomingMessage> GetMessagesToDispatch()
		{
			Logger.Debug("GetMessagesToDispatch");
			IEnumerable<IncomingMessage> result = null;
			_factory.ExecuteInUnitOfWork(uow =>
			{
				result = _repository
					.Where(uow, x => x.Status == Message.MessageStatus.ReceiverReceived)
					.OrderBy(x => x.CreatedTimeUtc).ToList();
			});
			
			return result;
		}

		public IEnumerable<IncomingMessage> GetByStatus(params Message.MessageStatus[] status)
		{
			Logger.Debug("GetByStatus");

			IEnumerable<IncomingMessage> result = null;
			_factory.ExecuteInUnitOfWork(uow => result = _repository
				                                             .Where(uow, x => status.Contains(x.Status))
				                                             .OrderBy(x => x.CreatedTimeUtc).ToList());
			return result;
		}

		public bool ContainsMessageFor(Guid messageId, Guid destinationComponent)
		{
			Logger.DebugFormat("ContainsMessageFor. messageId={0}, destinationComponent={1}",messageId,destinationComponent);

			if (messageId.IsEmpty() || destinationComponent.IsEmpty())
				throw new ArgumentException("the arguments cannot be empty");

			bool result = false;
			_factory.ExecuteInUnitOfWork(uow => result =
			                                    _repository.Any(uow,
			                                                    x =>
			                                                    x.MessageId == messageId &&
			                                                    x.SuscriptionHandlerId == destinationComponent));
			
			return result;
		}

		public IEnumerable<IncomingMessage> GetNonDistributedMessages()
		{
			Logger.Debug("GetNonDistributedMessages");

			IEnumerable<IncomingMessage> result = null;
			_factory.ExecuteInUnitOfWork(
				uow =>
				result =
				_repository.Where(uow,
				                  x => x.Status == Message.MessageStatus.ReceiverReceived && x.SuscriptionHandlerId == Guid.Empty)
				           .ToList());
			
			return result;
		}
	}
}