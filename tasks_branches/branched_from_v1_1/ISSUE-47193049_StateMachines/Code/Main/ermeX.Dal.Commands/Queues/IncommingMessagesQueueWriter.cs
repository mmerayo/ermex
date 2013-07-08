using System;
using System.Collections.Generic;
using System.Threading;

using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Connectivity;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Queues
{
	class IncommingQueueWriter : IWriteIncommingQueue
	{
		private readonly ILogger _logger ;

		private readonly IPersistRepository<IncomingMessage> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public IncommingQueueWriter(IPersistRepository<IncomingMessage> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
		{
			_logger = LogManager.GetLogger(typeof (IncommingQueueWriter), settings.ComponentId, LogComponent.DataServices);
			_logger.DebugFormat("cctor");
	        _repository = repository;
	        _factory = factory;
        }

		public void Save(IncomingMessage incomingMessage)
		{
			_logger.TraceFormat("Save. AppDomain={0} - Thread={1} - incomingMessage:{2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, incomingMessage);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, incomingMessage));
		}

		public void Save(IEnumerable<IncomingMessage> incomingMessages)
		{
			_logger.TraceFormat("Save. AppDomain={0} - Thread={1} - incomingMessages",AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, incomingMessages));
		}

		public void Remove(IncomingMessage incomingMessage)
		{
			//TODO: ADD THE THREAD AS A SUFFIX FOR ALL LOGGERS
			_logger.TraceFormat("Remove. AppDomain={0} - Thread={1} - incomingMessage:{2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, incomingMessage);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Remove(uow, incomingMessage));
		}
	}
}
