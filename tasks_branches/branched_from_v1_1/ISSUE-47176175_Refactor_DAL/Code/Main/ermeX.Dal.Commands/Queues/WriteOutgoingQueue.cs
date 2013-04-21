using System;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Queues
{
	internal class WriteOutgoingQueue : IWriteOutgoingQueue
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(WriteOutgoingQueue).FullName);
		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<OutgoingMessage> _repository;

		[Inject]
		public WriteOutgoingQueue(IPersistRepository<OutgoingMessage> repository, 
			IUnitOfWorkFactory factory, 
			IComponentSettings settings)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_factory = factory;
			_repository = repository;
		}

		public void RemoveExpiredMessages(TimeSpan expirationTime)
		{
			Logger.DebugFormat("RemoveExpiredMessages. expirationTime={0}",expirationTime);
			using (var uow = _factory.Create())
			{
				DateTime dateTime = DateTime.UtcNow - expirationTime;
				_repository.Remove(uow, x => x.CreatedTimeUtc <= dateTime);
				uow.Commit();
			}
		}

		public void Save(OutgoingMessage message)
		{
			Logger.DebugFormat("Save. message={0}", message);
			if (message.Status == Message.MessageStatus.NotSet)
			{
				Logger.Fatal("Save: message status wasnt set");
				throw new InvalidOperationException("Must set the status");
			}
			using (var uow = _factory.Create())
			{
				_repository.Save(uow, message);
				uow.Commit();
			}
		}
	}
}