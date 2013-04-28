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
	internal class ReaderOutgoingQueue : IReadOutgoingQueue
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ReaderOutgoingQueue).FullName);

		private readonly IReadOnlyRepository<OutgoingMessage> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public ReaderOutgoingQueue(IReadOnlyRepository<OutgoingMessage> repository,
		                           IUnitOfWorkFactory factory,
		                           IComponentSettings settings)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public IEnumerable<OutgoingMessage> GetItemsPendingSorted()
		{
			Logger.TraceFormat("GetItemsPendingSorted. AppDomain={0} - Thread={1}",AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId);
			IEnumerable<OutgoingMessage> result = null;
			_factory.ExecuteInUnitOfWork(true, uow => result =
			                                    _repository.Where(uow, x => x.Status != Message.MessageStatus.SenderFailed)
			                                               .OrderBy(x => x.Tries)
			                                               .ThenBy(x => x.CreatedTimeUtc).ToList());
			return result;
		}


		public IEnumerable<OutgoingMessage> GetExpiredMessages(TimeSpan expirationTime)
		{
			Logger.TraceFormat("GetExpiredMessages. expirationTime={0} - AppDomain={1} - Thread={2}", expirationTime,AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId);
			IEnumerable<OutgoingMessage> result = null;
			_factory.ExecuteInUnitOfWork(true, uow =>
				{
					DateTime dateTime = DateTime.UtcNow - expirationTime;

					result =
						_repository.Where(uow, x => x.CreatedTimeUtc <= dateTime)
						           .OrderBy(x => x.CreatedTimeUtc).ToList();
				});
			return result;

		}

		public bool ContainsMessageFor(Guid messageId, Guid destinationComponent)
		{
			Logger.TraceFormat("ContainsMessageFor. messageId={0} destinationComponent={1} - AppDomain={2} - Thread={3}", messageId, destinationComponent, AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);

			if (messageId.IsEmpty() || destinationComponent.IsEmpty())
				throw new ArgumentException("the arguments cannot be empty");

			bool result = false;
			_factory.ExecuteInUnitOfWork(true,uow => result =
			                                    _repository.Any(uow,
			                                                    x =>
			                                                    x.MessageId == messageId && x.PublishedTo == destinationComponent));
			return result;
		}

		public IEnumerable<OutgoingMessage> GetByStatus(params Message.MessageStatus[] status)
		{
			Logger.TraceFormat("GetByStatus.AppDomain={0} - Thread={1}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			IEnumerable<OutgoingMessage> result = null;
			_factory.ExecuteInUnitOfWork(true, uow => result = status.Length == 0
					         ? _repository.FetchAll(uow).ToList()
							 : _repository.Where(uow, x => status.Contains(x.Status)).ToList());
			
			return result;
		}
	}
}