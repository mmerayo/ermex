using System;
using System.Threading;
using Common.Logging;
using Ninject;

using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class ComponentsUpdater : ICanWriteComponents
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ComponentsUpdater).FullName);

		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<AppComponent> _repository;

		[Inject]
		public ComponentsUpdater(IPersistRepository<AppComponent> repository, IUnitOfWorkFactory factory)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_factory = factory;
			_repository = repository;
		}

		public void Save(AppComponent component)
		{
			Logger.TraceFormat("Save. AppDomain={0} - Thread={1} - component={2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, component);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, component));
		}
	}
}