using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Common.Logging;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.Linq;
using Remotion.Linq.Utilities;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.UnitOfWork;
using ermeX.Entities.Base;
using Ninject;

namespace ermeX.DAL.Repository
{
	internal class Repository<TEntity> : IPersistRepository<TEntity>
		 where TEntity : ModelBase
	{
		private readonly Guid _localComponentId;
		private readonly IExpressionHelper<TEntity> _expressionHelper;
		private readonly IUnitOfWorkFactory _implicitFactory;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Repository<TEntity>).FullName);

		[Inject]
		public Repository(IComponentSettings settings, 
			IExpressionHelper<TEntity> expressionHelper, IUnitOfWorkFactory implicitFactory )
		{
			Logger.DebugFormat("cctor. Thread={0}", Thread.CurrentThread.ManagedThreadId);
			if (settings.ComponentId==Guid.Empty)
				throw new ArgumentEmptyException("localComponentId");
			_localComponentId = settings.ComponentId;
			_expressionHelper = expressionHelper;
			_implicitFactory = implicitFactory;
		}

		public bool Save(TEntity entity)
		{
			Logger.DebugFormat("Save. entity={0}, thread={1}",entity, Thread.CurrentThread.ManagedThreadId);
			bool result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Save(unitOfWork, entity);
			return result;
		}
		

		public bool Save(IEnumerable<TEntity> items)
		{
			Logger.DebugFormat("Save entities thread={0}", Thread.CurrentThread.ManagedThreadId);
			bool result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Save(unitOfWork, items);
			return result;
		}

		public void Remove(int id)
		{
			Logger.DebugFormat("Remove id:{0} Thread={1}",id, Thread.CurrentThread.ManagedThreadId);
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, id);
		}

		public void Remove(TEntity entity)
		{
			Logger.DebugFormat("Remove by entity: {0} thread={1}", entity, Thread.CurrentThread.ManagedThreadId);
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, entity);
		}

		public void Remove(IEnumerable<TEntity> entities)
		{
			Logger.DebugFormat("Remove entitites. Thread={0}", Thread.CurrentThread.ManagedThreadId);
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, entities);
		}

		public void Remove(Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Remove expression: {0}",expression.ToString());
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, expression);
		}

		public int Count(Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Count expression: {0}", expression.ToString());
			int result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result=Count(unitOfWork, expression);

			return result;
		}

		public bool Any()
		{
			Logger.DebugFormat("Any Thread={0}", Thread.CurrentThread.ManagedThreadId);
			return FetchAll().Any();
		}

		public int Count()
		{
			Logger.DebugFormat("Count Thread={0}", Thread.CurrentThread.ManagedThreadId);
			return FetchAll().Count();
		}

		public void RemoveAll()
		{
			Logger.DebugFormat("RemoveAll  Thread={0}", Thread.CurrentThread.ManagedThreadId);
			using (var unitOfWork = _implicitFactory.Create(true))
				RemoveAll(unitOfWork);
		}

		public bool Save(IUnitOfWork unitOfWork, TEntity entity)
		{
			Logger.DebugFormat("Save: {0}  Thread={1}", Thread.CurrentThread.ManagedThreadId, entity);
			try
			{
				if (entity.ComponentOwner == Guid.Empty)
					throw new ArgumentEmptyException("entity.ComponentOwner");

				if (!CanSave(unitOfWork, entity))
					return false;

				if (entity.ComponentOwner == _localComponentId)
					entity.Version = DateTime.UtcNow.Ticks; //Keeps the version of the last updater

				entity.ComponentOwner = _localComponentId;
				if (entity.Id == 0)
					unitOfWork.Session.Save(entity);
				else
					unitOfWork.Session.Merge(entity);
				unitOfWork.Flush();
			}
			catch (GenericADOException e)
			{
				Logger.FatalFormat("Exception: Thread={0} - Exception: {1}" ,Thread.CurrentThread.ManagedThreadId,e);
				throw;
			}
			return true;
		}

		private bool CanSave(IUnitOfWork unitOfWork, TEntity entity)
		{
			var findByBizKey = _expressionHelper.GetFindByBizKey(entity);
			var item = SingleOrDefault(unitOfWork, findByBizKey);
			//is true if it didnt exist or the current one is new or if it existed but is older than the new version
			bool result = item == null || entity.Version == 0 || item.Version <= entity.Version;
			if (item != null)
				entity.Id = item.Id;

			return result; //Can save if it didnt exist or the version is newer
		}

		public bool Save(IUnitOfWork unitOfWork, IEnumerable<TEntity> items)
		{
			Logger.Debug("Save entities");
			foreach (TEntity item in items)
				Save(unitOfWork, item);
			return true;
		}

		public void RemoveAll(IUnitOfWork unitOfWork)
		{
			Logger.Debug("RemoveAll");
			var fetchAll = FetchAll(unitOfWork);
			Remove(unitOfWork, fetchAll);
		}

		public void Remove(IUnitOfWork unitOfWork)
		{
			Logger.Debug("Remove");
			Remove(unitOfWork, 0);
		}

		public void Remove(IUnitOfWork unitOfWork, int id)
		{
			Logger.DebugFormat("Remove Id:{0}",id);
			var toRemove = Single(unitOfWork, id);
			Remove(unitOfWork, toRemove);
		}

		public void Remove(IUnitOfWork unitOfWork, TEntity entity)
		{
			Logger.DebugFormat("Remove: Entity: {0} - ThreadId=",entity,Thread.CurrentThread.ManagedThreadId);
			unitOfWork.Session.Delete(entity);
			unitOfWork.Flush();
		}

		public void Remove(IUnitOfWork unitOfWork, IEnumerable<TEntity> entities)
		{
			Logger.DebugFormat("Remove entities. Thread={0}", Thread.CurrentThread.ManagedThreadId);
			foreach (var entity in entities)
				Remove(unitOfWork, entity);
		}

		public void Remove(IUnitOfWork unitOfWork,Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Remove: {0}", expression);
			IQueryable<TEntity> queryable = Where(unitOfWork,expression);
			Remove(unitOfWork, queryable);
		}

		public IQueryable<TEntity> FetchAll(bool includingOtherComponents = false)
		{
			Logger.DebugFormat("FetchAll, includingOtherComponents= {0}, Thread={1}", includingOtherComponents, Thread.CurrentThread.ManagedThreadId);
			IQueryable<TEntity> result;
			using(var unitOfWork = _implicitFactory.Create(true))
				result = FetchAll(unitOfWork, includingOtherComponents);
			return result;
		}

		public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Where {0}", expression);

			IQueryable<TEntity> result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Where(unitOfWork, expression);
			return result;
		}

		public TEntity Single(int id)
		{
			Logger.DebugFormat("Single Id:{0}, thread={1}", id, Thread.CurrentThread.ManagedThreadId);
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Single(unitOfWork, id);
			return result;
		}

		public TEntity Single(Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Single expression: {0}", expression);
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Single(unitOfWork, expression);
			return result;
		}

		public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("SingleOrDefault expression: {0}", expression);
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = SingleOrDefault(unitOfWork, expression);
			return result;
		}

		public TEntity SingleOrDefault(int id)
		{
			Logger.DebugFormat("SingleOrDefault id: {0}", id);
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = SingleOrDefault(unitOfWork, id);
			return result;
		}

		public TResult GetMax<TResult>(string propertyName)
		{
			Logger.DebugFormat("GetMax<TResult> propertyName: {0}", propertyName);
			TResult result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = GetMax<TResult>(unitOfWork, propertyName);
			return result;
		}

		public bool Any(Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Any expression: {0}", expression);
			bool result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Any(unitOfWork, expression);
			return result;
		}

		public IQueryable<TEntity> FetchAll(IUnitOfWork unitOfWork, bool includingOtherComponents = false)
		{
			Logger.DebugFormat("FetchAll includingOtherComponents: {0}", includingOtherComponents);
			var absolute = unitOfWork.Session.Query<TEntity>();
			if(!includingOtherComponents)
				return absolute.Where(IsLocalPredicate).ToList().AsQueryable();
			return absolute;
		}

		public TEntity Single(IUnitOfWork unitOfWork, int id)
		{
			Logger.DebugFormat("Single id: {0}", id);
			var result = unitOfWork.Session.Get<TEntity>(id);
			if (result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The entity is owned by another component");
			return result;
		}

		public TEntity Single(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Single expression: {0} thread={1}", expression, Thread.CurrentThread.ManagedThreadId);
			return Where(unitOfWork,expression).Single();
		}


		public TEntity SingleOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("SingleOrDefault expression: {0}, thread={1}", expression, Thread.CurrentThread.ManagedThreadId);
			var queryable = Where(unitOfWork,expression);
			TEntity singleOrDefault = queryable.SingleOrDefault();
			return singleOrDefault;
		}

		public TEntity SingleOrDefault(IUnitOfWork unitOfWork, int id)
		{
			Logger.DebugFormat("SingleOrDefault id: {0} thread={1}", id, Thread.CurrentThread.ManagedThreadId);
			var result = unitOfWork.Session.Get<TEntity>(id);
			if (result!=null && result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The entity is owned by another component");
			return result;
		}

		public TResult GetMax<TResult>(IUnitOfWork unitOfWork, string propertyName)
		{
			Logger.DebugFormat("GetMax propertyName: {0} thread={1}", propertyName, Thread.CurrentThread.ManagedThreadId);
			return
				unitOfWork.Session.QueryOver<TEntity>()
				        .Where(IsLocalPredicate)
				        .Select(Projections.Max(propertyName))
				        .SingleOrDefault<TResult>();

		}

		public bool Any(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Any expression: {0}", expression);
			return Where(unitOfWork,expression).Any();
		}

		public IQueryable<TEntity> Where(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Where expression: {0}", expression);
			return unitOfWork.Session.Query<TEntity>().Where(IsLocalPredicate).Where(expression).ToList().AsQueryable(); //TODO: IMPROVE USING AND BETWEEN BOTH EXPRESSIONS
		}

		private Expression<Func<TEntity, bool>> IsLocalPredicate
		{
			get { return x=>x.ComponentOwner==_localComponentId; }
		}

		public int Count(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			Logger.DebugFormat("Count expression: {0} thread={1}", expression, Thread.CurrentThread.ManagedThreadId);
			return Where(unitOfWork,expression).Count();
		}

		public bool Any(IUnitOfWork unitOfWork)
		{
			Logger.DebugFormat("Any. Thread={0}", Thread.CurrentThread.ManagedThreadId);
			return FetchAll(unitOfWork).Any();
		}

		public int Count(IUnitOfWork unitOfWork)
		{
			Logger.DebugFormat("Count. Thread={0}", Thread.CurrentThread.ManagedThreadId);
			return FetchAll(unitOfWork).Count();
		}
	}
}
