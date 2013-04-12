using System;
using Ninject;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.DataAccess.Repository;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces.Component;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class ComponentsUpdater : ICanWriteComponents
	{
		private readonly IUnitOfWorkFactory _factory;
		private readonly IPersistRepository<AppComponent> _repository;

		[Inject]
		public ComponentsUpdater(IPersistRepository<AppComponent> repository, IUnitOfWorkFactory factory)
		{
			_factory = factory;
			_repository = repository;
		}

		public void SetComponentRunningStatus(Guid componentId, ComponentStatus newStatus, bool exchangedDefinitions = false)
		{
			using (var uow = _factory.Create())
			{
				var localComponent = _repository.Single(uow.Session, x => x.ComponentId == componentId);
				localComponent.IsRunning = newStatus == ComponentStatus.Running;
				localComponent.ExchangedDefinitions = exchangedDefinitions;

				Save(localComponent);
				uow.Commit();
			}

		}

		public void Save(AppComponent component)
		{
			using (var uow = _factory.Create())
			{
				_repository.Save(uow.Session, component);
				uow.Commit();
			}
		}
	}
}