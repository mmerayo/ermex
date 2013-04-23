using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Repository;
using ermeX.DAL.UnitOfWork;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Commands.Component
{
	internal sealed class ComponentsReader : ICanReadComponents
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (ComponentsReader).FullName);
		private readonly IUnitOfWorkFactory _factory;
		private readonly IComponentSettings _settings;
		private IReadOnlyRepository<AppComponent> Repository { get; set; }

		[Inject]
		public ComponentsReader(IReadOnlyRepository<AppComponent> repository, IUnitOfWorkFactory factory,
		                        IComponentSettings settings)
		{
			Logger.DebugFormat("cctor. Thread={0}",Thread.CurrentThread.ManagedThreadId);
			
			_factory = factory;
			_settings = settings;
			Repository = repository;
		}

		public IEnumerable<AppComponent> FetchAll()
		{
			Logger.Debug("FetchAll");
			List<AppComponent> result=null;
			_factory.ExecuteInUnitOfWork(true,uow => result = Repository.FetchAll(uow).ToList());
			
			return result;
		}

		public IEnumerable<AppComponent> FetchOtherComponents()
		{
			Logger.Debug("FetchOtherComponents");
			List<AppComponent> result=null;
			_factory.ExecuteInUnitOfWork(true,
				uow => result = Repository.Where(uow, x => x.ComponentId != _settings.ComponentId).ToList());
			
			return result;
		}

		public IEnumerable<AppComponent> FetchOtherComponentsNotExchangedDefinitions(bool running = false)
		{
			Logger.DebugFormat("FetchOtherComponentsNotExchangedDefinitions. running={0}", running);
			List<AppComponent> result=null;
			_factory.ExecuteInUnitOfWork(true,
				uow =>
				result = Repository.Where(uow, x => x.ComponentId != _settings.ComponentId
				                                    && !x.ExchangedDefinitions
				                                    && x.IsRunning == running).ToList());
			
			return result;
		}

		public AppComponent Fetch(Guid componentId)
		{
			Logger.DebugFormat("Fetch. componentId={0}", componentId);
			AppComponent result=null;
			_factory.ExecuteInUnitOfWork(true, uow => result = Repository.SingleOrDefault(uow, x => x.ComponentId == componentId));
			
			return result;
		}
	}
}