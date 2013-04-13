using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Remotion.Linq.Utilities;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.UoW;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Base;
using Ninject;

namespace ermeX.DAL.DataAccess.Repository
{
	internal class Repository<TEntity> : IPersistRepository<TEntity>
		 where TEntity : ModelBase
	{
		private readonly Guid _localComponentId;
		private readonly IExpressionHelper<TEntity> _expressionHelper;
		private readonly IUnitOfWorkFactory _implicitFactory;

		[Inject]
		public Repository(IComponentSettings settings, 
			IExpressionHelper<TEntity> expressionHelper, IUnitOfWorkFactory implicitFactory )
		{
			if (settings.ComponentId==Guid.Empty)
				throw new ArgumentEmptyException("localComponentId");
			_localComponentId = settings.ComponentId;
			_expressionHelper = expressionHelper;
			_implicitFactory = implicitFactory;
		}

		public bool Save(TEntity entity)
		{
			bool result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Save(unitOfWork, entity);
			return result;
		}
		

		public bool Save(IEnumerable<TEntity> items)
		{
			bool result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Save(unitOfWork, items);
			return result;
		}

		public void Remove(int id)
		{
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, id);
		}

		public void Remove(TEntity entity)
		{
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, entity);
		}

		public void Remove(IEnumerable<TEntity> entities)
		{
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, entities);
		}

		public void Remove(Expression<Func<TEntity, bool>> expression)
		{
			using (var unitOfWork = _implicitFactory.Create(true))
				Remove(unitOfWork, expression);
		}

		public int Count(Expression<Func<TEntity, bool>> expression)
		{
			int result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result=Count(unitOfWork, expression);

			return result;
		}

		public bool Any()
		{
			return FetchAll().Any();
		}

		public int Count()
		{
			return FetchAll().Count();
		}

		public void RemoveAll()
		{
			using (var unitOfWork = _implicitFactory.Create(true))
				RemoveAll(unitOfWork);
		}

		public bool Save(IUnitOfWork unitOfWork, TEntity entity)
		{
			if (entity.ComponentOwner == Guid.Empty)
				throw new ArgumentEmptyException("entity.ComponentOwner");

			if (!CanSave(unitOfWork,entity))
				return false;

			if (entity.ComponentOwner == _localComponentId)
				entity.Version = DateTime.UtcNow.Ticks; //Keeps the version of the last updater

			entity.ComponentOwner = _localComponentId;

			unitOfWork.Session.SaveOrUpdate(entity);
			return true;
		}

		private bool CanSave(IUnitOfWork unitOfWork,TEntity entity)
		{
			var item = SingleOrDefault(unitOfWork,_expressionHelper.GetFindByBizKey(entity));
			bool result = item == null || item.Version <= entity.Version;
			if(item!=null)
				unitOfWork.Session.Evict(item);

			return result; //Can save if it didnt exist or the version is newer
		}
		
		public bool Save(IUnitOfWork unitOfWork, IEnumerable<TEntity> items)
		{
			foreach (TEntity item in items)
				Save(unitOfWork, item);
			return true;
		}

		public void RemoveAll(IUnitOfWork unitOfWork)
		{
			var fetchAll = FetchAll(unitOfWork);
			Remove(unitOfWork, fetchAll);
		}

		public void Remove(IUnitOfWork unitOfWork)
		{
			Remove(unitOfWork, 0);
		}

		public void Remove(IUnitOfWork unitOfWork, int id)
		{
			var toRemove = Single(unitOfWork, id);
			Remove(unitOfWork, toRemove);
		}

		public void Remove(IUnitOfWork unitOfWork, TEntity entity)
		{
			unitOfWork.Session.Delete(entity);
		}

		public void Remove(IUnitOfWork unitOfWork, IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
				Remove(unitOfWork, entity);
		}

		public void Remove(IUnitOfWork unitOfWork,Expression<Func<TEntity, bool>> expression)
		{
			IQueryable<TEntity> queryable = Where(unitOfWork,expression);
			Remove(unitOfWork, queryable);
		}

		public IQueryable<TEntity> FetchAll(bool includingOtherComponents = false)
		{
			IQueryable<TEntity> result;
			using(var unitOfWork = _implicitFactory.Create(true))
				result = FetchAll(unitOfWork, includingOtherComponents);
			return result;
		}

		public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression)
		{
			IQueryable<TEntity> result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Where(unitOfWork, expression);
			return result;
		}

		public TEntity Single(int id)
		{
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Single(unitOfWork, id);
			return result;
		}

		public TEntity Single(Expression<Func<TEntity, bool>> expression)
		{
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Single(unitOfWork, expression);
			return result;
		}

		public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression)
		{
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = SingleOrDefault(unitOfWork, expression);
			return result;
		}

		public TEntity SingleOrDefault(int id)
		{
			TEntity result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = SingleOrDefault(unitOfWork, id);
			return result;
		}

		public TResult GetMax<TResult>(string propertyName)
		{
			TResult result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = GetMax<TResult>(unitOfWork, propertyName);
			return result;
		}

		public bool Any(Expression<Func<TEntity, bool>> expression)
		{
			bool result;
			using (var unitOfWork = _implicitFactory.Create(true))
				result = Any(unitOfWork, expression);
			return result;
		}

		public IQueryable<TEntity> FetchAll(IUnitOfWork unitOfWork, bool includingOtherComponents = false)
		{
			var absolute = unitOfWork.Session.Query<TEntity>();
			if(!includingOtherComponents)
				return absolute.Where(IsLocalPredicate).ToList().AsQueryable();
			return absolute;
		}

		public TEntity Single(IUnitOfWork unitOfWork, int id)
		{
			var result = unitOfWork.Session.Get<TEntity>(id);
			if (result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The entity is owned by another component");
			return result;
		}

		public TEntity Single(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitOfWork,expression).Single();
		}


		public TEntity SingleOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitOfWork,expression).SingleOrDefault();
		}

		public TEntity SingleOrDefault(IUnitOfWork unitOfWork, int id)
		{
			var result = unitOfWork.Session.Get<TEntity>(id);
			if (result!=null && result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The entity is owned by another component");
			return result;
		}

		public TResult GetMax<TResult>(IUnitOfWork unitOfWork, string propertyName)
		{
			return
				unitOfWork.Session.QueryOver<TEntity>()
				        .Where(IsLocalPredicate)
				        .Select(Projections.Max(propertyName))
				        .SingleOrDefault<TResult>();

		}

		public bool Any(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitOfWork,expression).Any();
		}

		public IQueryable<TEntity> Where(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			return unitOfWork.Session.Query<TEntity>().Where(IsLocalPredicate).Where(expression); //TODO: IMPROVE USING AND BETWEEN BOTH EXPRESSIONS
		}

		private Expression<Func<TEntity, bool>> IsLocalPredicate
		{
			get { return x=>x.ComponentOwner==_localComponentId; }
		}

		public int Count(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitOfWork,expression).Count();
		}

		public bool Any(IUnitOfWork unitOfWork)
		{
			return FetchAll(unitOfWork).Any();
		}

		public int Count(IUnitOfWork unitOfWork)
		{
			return FetchAll(unitOfWork).Count();
		}
	}
}
