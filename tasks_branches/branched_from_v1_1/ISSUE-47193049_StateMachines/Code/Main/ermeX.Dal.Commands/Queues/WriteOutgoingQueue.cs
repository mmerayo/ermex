using System;
using System.Threading;

using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Queues
{
	internal class WriteOutgoingQueue : IWriteOutgoingQueue
	{
		private readonly ILogger _logger;
		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<OutgoingMessage> _repository;

		[Inject]
		public WriteOutgoingQueue(IPersistRepository<OutgoingMessage> repository, 
			IUnitOfWorkFactory factory, 
			IComponentSettings settings)
		{
			_logger = LogManager.GetLogger<WriteOutgoingQueue>(settings.ComponentId, LogComponent.DataServices);
			_logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_factory = factory;
			_repository = repository;
		}

		public void RemoveExpiredMessages(TimeSpan expirationTime)
		{
			_logger.TraceFormat("RemoveExpiredMessages. expirationTime={0} - AppDomain={1} - Thread={2}", expirationTime,AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId);
			_factory.ExecuteInUnitOfWork(false, uow =>
				{
					DateTime dateTime = DateTime.UtcNow - expirationTime;
					_repository.Remove(uow, x => x.CreatedTimeUtc <= dateTime);
				});
		}

		public void Save(OutgoingMessage message)
		{
			_logger.TraceFormat("Save.AppDomain={0} - Thread={1} -  message={2}",AppDomain.CurrentDomain.Id,Thread.CurrentThread.ManagedThreadId, message);
			if (message.Status == Message.MessageStatus.NotSet)
			{
				_logger.Fatal("Save: message status wasnt set");
				throw new InvalidOperationException("Must set the status");
			}

			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, message));
		}
	}
}