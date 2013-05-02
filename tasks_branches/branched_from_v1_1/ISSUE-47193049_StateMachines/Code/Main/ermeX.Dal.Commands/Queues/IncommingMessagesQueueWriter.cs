using System;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Connectivity;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Queues
{
	class IncommingQueueWriter : IWriteIncommingQueue
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(IncommingQueueWriter).FullName);

		private readonly IPersistRepository<IncomingMessage> _repository;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public IncommingQueueWriter(IPersistRepository<IncomingMessage> repository,
			IUnitOfWorkFactory factory,
			IComponentSettings settings)
        {
			Logger.DebugFormat("cctor");
	        _repository = repository;
	        _factory = factory;
        }

		public void Save(IncomingMessage incomingMessage)
		{
			Logger.TraceFormat("Save. AppDomain={0} - Thread={1} - incomingMessage:{2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, incomingMessage);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, incomingMessage));
		}

		public void Save(IEnumerable<IncomingMessage> incomingMessages)
		{
			Logger.TraceFormat("Save. AppDomain={0} - Thread={1} - incomingMessages",AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, incomingMessages));
		}

		public void Remove(IncomingMessage incomingMessage)
		{
			//TODO: ADD THE THREAD AS A SUFFIX FOR ALL LOGGERS
			Logger.TraceFormat("Remove. AppDomain={0} - Thread={1} - incomingMessage:{2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, incomingMessage);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Remove(uow, incomingMessage));
		}
	}
}
