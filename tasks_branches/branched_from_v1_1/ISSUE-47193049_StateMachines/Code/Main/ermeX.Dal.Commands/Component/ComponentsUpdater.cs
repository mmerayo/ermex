using System;
using System.Threading;

using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class ComponentsUpdater : ICanWriteComponents
	{
		private static ILogger _logger ;

		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<AppComponent> _repository;

		[Inject]
		public ComponentsUpdater(IPersistRepository<AppComponent> repository,
			IUnitOfWorkFactory factory,IComponentSettings settings)
		{
			_logger = LogManager.GetLogger<ComponentsUpdater>(settings.ComponentId, LogComponent.DataServices);
			_logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			_factory = factory;
			_repository = repository;
		}

		public void Save(AppComponent component)
		{
			_logger.TraceFormat("Save. AppDomain={0} - Thread={1} - component={2}", AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, component);
			_factory.ExecuteInUnitOfWork(false, uow => _repository.Save(uow, component));
		}
	}
}