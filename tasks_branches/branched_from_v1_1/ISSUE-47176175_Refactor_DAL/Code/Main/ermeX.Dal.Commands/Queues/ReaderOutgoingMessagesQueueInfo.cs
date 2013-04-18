using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
			Logger.Debug("GetItemsPendingSorted");
			IEnumerable<OutgoingMessage> result;
			using (var uow = _factory.Create())
			{
				result =
					_repository.Where(uow, x => x.Status != Message.MessageStatus.SenderFailed)
					           .OrderBy(x => x.Tries)
					           .ThenBy(x => x.CreatedTimeUtc).ToList();
				uow.Commit();
			}
			return result;
		}


		public IEnumerable<OutgoingMessage> GetExpiredMessages(TimeSpan expirationTime)
		{
			Logger.DebugFormat("GetExpiredMessages. expirationTime={0}",expirationTime);
			IEnumerable<OutgoingMessage> result;
			using (var uow = _factory.Create())
			{
				DateTime dateTime = DateTime.UtcNow - expirationTime;

				result =
					_repository.Where(uow, x => x.CreatedTimeUtc <= dateTime)
					           .OrderBy(x => x.CreatedTimeUtc).ToList();
				uow.Commit();
			}
			return result;

		}


		public bool ContainsMessageFor(Guid messageId, Guid destinationComponent)
		{
			Logger.DebugFormat("ContainsMessageFor. messageId={0} destinationComponent={1}", messageId,destinationComponent);

			if (messageId.IsEmpty() || destinationComponent.IsEmpty())
				throw new ArgumentException("the arguments cannot be empty");

			bool result;
			using (var uow = _factory.Create())
			{
				result =
					_repository.Any(uow, x => x.MessageId == messageId && x.PublishedTo == destinationComponent);
				uow.Commit();
			}
			return result;
		}

		public IEnumerable<OutgoingMessage> GetByStatus(params Message.MessageStatus[] status)
		{
			Logger.Debug("GetByStatus");
			IEnumerable<OutgoingMessage> result;
			using (var uow = _factory.Create())
			{
				result = status.Length == 0
					         ? _repository.FetchAll(uow).ToList()
							 : _repository.Where(uow, x => status.Contains(x.Status)).ToList();
				uow.Commit();
			}
			return result;
		}
	}
}