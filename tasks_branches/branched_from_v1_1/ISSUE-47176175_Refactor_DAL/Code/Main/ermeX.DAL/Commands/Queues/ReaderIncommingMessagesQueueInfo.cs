using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Queues;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Queues
{
	class ReaderIncommingQueue : IReadIncommingQueue
	{
		private readonly IReadOnlyRepository<IncomingMessage> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public ReaderIncommingQueue(IReadOnlyRepository<IncomingMessage> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
        {
	        _repository = repository;
	        _factory = factory;
        }

		public IncomingMessage GetNextDispatchableItem(int maxLatency)
		{
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