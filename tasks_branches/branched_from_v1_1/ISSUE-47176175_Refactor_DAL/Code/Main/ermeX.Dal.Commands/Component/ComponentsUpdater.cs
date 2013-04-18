using System;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UnitOfWork;
using ermeX.DAL.Interfaces.Component;
using ermeX.Entities.Entities;

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

		public void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false)
		{
			Logger.DebugFormat("SetComponentRunningStatus. componentId={0}, newStatus={1}, exchangedDefinitions={2}", componentId,
			                   newStatus, exchangedDefinitions);
			using (var uow = _factory.Create())
			{
				var localComponent = _repository.Single(uow, x => x.ComponentId == componentId);
				localComponent.IsRunning = newStatus == ComponentStatus.Running;
				localComponent.ExchangedDefinitions = exchangedDefinitions;

				_repository.Save(uow, localComponent);
				uow.Commit();
			}

		}

		public void Save(AppComponent component)
		{
			Logger.DebugFormat("Save. component={0}", component);
			using (var uow = _factory.Create())
			{
				_repository.Save(uow, component);
				uow.Commit();
			}
		}
	}
}