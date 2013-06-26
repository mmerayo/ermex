using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Queues
{
	class ReaderIncommingQueue : IReadIncommingQueue
	{
		private static readonly ILogger Logger = LogManager.GetLogger(typeof(ReaderIncommingQueue).FullName);
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
			Logger.TraceFormat("GetNextDispatchableItem. maxLatency={0} - AppDomain={1} - Thread={2} ", maxLatency,AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId);
			
			IncomingMessage result = null;
			_factory.ExecuteInUnitOfWork(true, uow =>
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
			Logger.TraceFormat("GetMessagesToDispatch. - AppDomain={0} - Thread={1}",AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId);
			IEnumerable<IncomingMessage> result = null;
			_factory.ExecuteInUnitOfWork(true,uow =>
			{
				result = _repository
					.Where(uow, x => x.Status == Message.MessageStatus.ReceiverReceived)
					.OrderBy(x => x.CreatedTimeUtc).ToList();
			});
			
			return result;
		}

		public IEnumerable<IncomingMessage> GetByStatus(params Message.MessageStatus[] status)
		{
			Logger.TraceFormat("GetByStatus. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);

			IEnumerable<IncomingMessage> result = null;
			_factory.ExecuteInUnitOfWork(true, uow => result = _repository
				                                             .Where(uow, x => status.Contains(x.Status))
				                                             .OrderBy(x => x.CreatedTimeUtc).ToList());
			return result;
		}

		public bool ContainsMessageFor(Guid messageId, Guid destinationComponent)
		{
			Logger.TraceFormat("ContainsMessageFor. messageId={0}, destinationComponent={1}- AppDomain={2} - Thread={3}", messageId, destinationComponent,AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId);

			if (messageId.IsEmpty() || destinationComponent.IsEmpty())
				throw new ArgumentException("the arguments cannot be empty");

			bool result = false;
			_factory.ExecuteInUnitOfWork(true, uow => result =
			                                    _repository.Any(uow,
			                                                    x =>
			                                                    x.MessageId == messageId &&
			                                                    x.SuscriptionHandlerId == destinationComponent));
			
			return result;
		}

		public IEnumerable<IncomingMessage> GetNonDistributedMessages()
		{
			Logger.TraceFormat("GetNonDistributedMessages. AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);

			IEnumerable<IncomingMessage> result = null;
			_factory.ExecuteInUnitOfWork(true,
				uow =>
				result =
				_repository.Where(uow,
				                  x => x.Status == Message.MessageStatus.ReceiverReceived && x.SuscriptionHandlerId == Guid.Empty)
				           .ToList());
			
			return result;
		}
	}
}