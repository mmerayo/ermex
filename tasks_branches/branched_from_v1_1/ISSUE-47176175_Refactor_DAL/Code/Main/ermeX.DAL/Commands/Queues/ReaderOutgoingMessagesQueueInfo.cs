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
	internal class ReaderOutgoingQueue : IReadOutgoingQueue
	{
		private readonly IReadOnlyRepository<OutgoingMessage> _repository;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;

		[Inject]
		public ReaderOutgoingQueue(IReadOnlyRepository<OutgoingMessage> repository,
		                           IUnitOfWorkFactory factory,
		                           IComponentSettings settings)
		{
			_repository = repository;
			_factory = factory;
			_settings = settings;
		}

		public IEnumerable<OutgoingMessage> GetItemsPendingSorted()
		{
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