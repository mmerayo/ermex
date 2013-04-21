﻿using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Commands.Connectivity;
using ermeX.DAL.Interfaces.Queues;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Entities.Entities;

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
			Logger.DebugFormat("Save. incomingMessage:{0} - ThreadId:{1}",incomingMessage,Thread.CurrentThread.ManagedThreadId);
			using (var uow = _factory.Create())
			{
				_repository.Save(uow, incomingMessage);
				uow.Commit();
			}
		}

		public void Save(IEnumerable<IncomingMessage> incomingMessages)
		{
			Logger.DebugFormat("Save. incomingMessages");
			using (var uow = _factory.Create())
			{
				_repository.Save(uow, incomingMessages);
				uow.Commit();
			}
		}

		public void Remove(IncomingMessage incomingMessage)
		{
			//TODO: ADD THE THREAD AS A SUFFIX FOR ALL LOGGERS
			Logger.DebugFormat("Remove. incomingMessage:{0} - ThreadId:{1}", incomingMessage,Thread.CurrentThread.ManagedThreadId);
			using (var uow = _factory.Create())
			{
				_repository.Remove(uow, incomingMessage);
				uow.Commit();
			}
			
		}

	}
}
