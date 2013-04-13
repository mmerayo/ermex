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

		[Inject]
		public Repository(IComponentSettings settings, 
			IExpressionHelper<TEntity> expressionHelper )
		{
			if (settings.ComponentId==Guid.Empty)
				throw new ArgumentEmptyException("localComponentId");
			_localComponentId = settings.ComponentId;
			_expressionHelper = expressionHelper;
		}

		public bool Save(IUnitOfWork unitofWork, TEntity entity)
		{
			if (entity.ComponentOwner == Guid.Empty)
				throw new ArgumentEmptyException("entity.ComponentOwner");

			if (!CanSave(unitofWork,entity))
				return false;


			if (entity.ComponentOwner == _localComponentId)
				entity.Version = DateTime.UtcNow.Ticks; //Keeps the version of the last updater

			entity.ComponentOwner = _localComponentId;

			unitofWork.Session.SaveOrUpdate(entity);
			return true;
		}

		private bool CanSave(IUnitOfWork unitofWork,TEntity entity)
		{
			var item = SingleOrDefault(unitofWork,_expressionHelper.GetFindByBizKey(entity));
			bool result = item == null || item.Version <= entity.Version;
			if(item!=null)
				unitofWork.Session.Evict(item);

			return result; //Can save if it didnt exist or the version is newer
		}
		
		public bool Save(IUnitOfWork unitofWork, IEnumerable<TEntity> items)
		{
			foreach (TEntity item in items)
				Save(unitofWork, item);
			return true;
		}

		public void RemoveAll(IUnitOfWork unitofWork)
		{
			var fetchAll = FetchAll(unitofWork);
			Remove(unitofWork, fetchAll);
		}

		public void Remove(IUnitOfWork unitofWork)
		{
			Remove(unitofWork, 0);
		}

		public void Remove(IUnitOfWork unitofWork, int id)
		{
			var toRemove = Single(unitofWork, id);
			Remove(unitofWork, toRemove);
		}

		public void Remove(IUnitOfWork unitofWork, TEntity entity)
		{
			unitofWork.Session.Delete(entity);
		}

		public void Remove(IUnitOfWork unitofWork, IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
				Remove(unitofWork, entity);
		}

		public void Remove(IUnitOfWork unitofWork,Expression<Func<TEntity, bool>> expression)
		{
			IQueryable<TEntity> queryable = Where(unitofWork,expression);
			Remove(unitofWork, queryable);
		}

		public IQueryable<TEntity> FetchAll(IUnitOfWork unitofWork, bool includingOtherComponents = false)
		{
			var absolute = unitofWork.Session.Query<TEntity>();
			if(!includingOtherComponents)
				return absolute.Where(IsLocalPredicate);
			return absolute;
		}

		public TEntity Single(IUnitOfWork unitofWork, int id)
		{
			var result = unitofWork.Session.Get<TEntity>(id);
			if (result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The entity is owned by another component");
			return result;
		}

		public TEntity Single(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitofWork,expression).Single();
		}


		public TEntity SingleOrDefault(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitofWork,expression).SingleOrDefault();
		}

		public TEntity SingleOrDefault(IUnitOfWork unitofWork, int id)
		{
			var result = unitofWork.Session.Get<TEntity>(id);
			if (result!=null && result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The entity is owned by another component");
			return result;
		}

		public TResult GetMax<TResult>(IUnitOfWork unitofWork, string propertyName)
		{
			return
				unitofWork.Session.QueryOver<TEntity>()
				        .Where(IsLocalPredicate)
				        .Select(Projections.Max(propertyName))
				        .SingleOrDefault<TResult>();

		}

		public bool Any(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitofWork,expression).Any();
		}

		public IQueryable<TEntity> Where(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression)
		{
			return unitofWork.Session.Query<TEntity>().Where(IsLocalPredicate).Where(expression); //TODO: IMPROVE USING AND BETWEEN BOTH EXPRESSIONS
		}

		private Expression<Func<TEntity, bool>> IsLocalPredicate
		{
			get { return x=>x.ComponentOwner==_localComponentId; }
		}

		public int Count(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression)
		{
			return Where(unitofWork,expression).Count();
		}
		
	}
}
