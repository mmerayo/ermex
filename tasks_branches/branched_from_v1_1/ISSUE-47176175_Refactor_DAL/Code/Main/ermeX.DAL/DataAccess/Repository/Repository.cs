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

		public bool Save(ISession session, TEntity entity)
		{
			if (entity.ComponentOwner == Guid.Empty)
				throw new ArgumentEmptyException("entity.ComponentOwner");

			if (!CanSave(session,entity))
				return false;


			if (entity.ComponentOwner == _localComponentId)
				entity.Version = DateTime.UtcNow.Ticks; //Keeps the version of the last updater

			entity.ComponentOwner = _localComponentId;

			session.SaveOrUpdate(entity);
			return true;
		}

		private bool CanSave(ISession session,TEntity entity)
		{
			var item = SingleOrDefault(session,_expressionHelper.GetFindByBizKey(entity));
			bool result = item == null || item.Version <= entity.Version;
			if(item!=null)
				session.Evict(item);

			return result; //Can save if it didnt exist or the version is newer
		}
		
		public bool Save(ISession session, IEnumerable<TEntity> items)
		{
			foreach (TEntity item in items)
				Save(session, item);
			return true;
		}

		public void RemoveAll(ISession session)
		{
			var fetchAll = FetchAll(session);
			Remove(session, fetchAll);
		}

		public void Remove(ISession session)
		{
			Remove(session, 0);
		}

		public void Remove(ISession session, int id)
		{
			var toRemove = Single(session, id);
			Remove(session, toRemove);
		}

		public void Remove(ISession session, TEntity entity)
		{
			session.Delete(entity);
		}

		public void Remove(ISession session, IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
				Remove(session, entity);
		}

		public void Remove(ISession session,Expression<Func<TEntity, bool>> expression)
		{
			IQueryable<TEntity> queryable = Where(session,expression);
			Remove(session, queryable);
		}

		public IQueryable<TEntity> FetchAll(ISession session, bool includingOtherComponents = false)
		{
			var absolute = session.Query<TEntity>();
			if(!includingOtherComponents)
				return absolute.Where(IsLocalPredicate);
			return absolute;
		}

		public TEntity Single(ISession session, int id)
		{
			var result = session.Get<TEntity>(id);
			if (result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The id is owned by another component");
			return result;
		}

		public TEntity Single(ISession session, Expression<Func<TEntity, bool>> expression)
		{
			return Where(session,expression).Single();
		}


		public TEntity SingleOrDefault(ISession session, Expression<Func<TEntity, bool>> expression)
		{
			return Where(session,expression).SingleOrDefault();
		}

		public TResult GetMax<TResult>(ISession session, string propertyName)
		{
			return
				session.QueryOver<TEntity>()
				        .Where(IsLocalPredicate)
				        .Select(Projections.Max(propertyName))
				        .SingleOrDefault<TResult>();

		}

		public bool Any(ISession session, Expression<Func<TEntity, bool>> expression)
		{
			return Where(session,expression).Any();
		}

		public IQueryable<TEntity> Where(ISession session, Expression<Func<TEntity, bool>> expression)
		{
			return session.Query<TEntity>().Where(IsLocalPredicate).Where(expression); //TODO: IMPROVE USING AND BETWEEN BOTH EXPRESSIONS
		}

		private Expression<Func<TEntity, bool>> IsLocalPredicate
		{
			get { return x=>x.ComponentOwner==_localComponentId; }
		}

		public int Count(ISession session, Expression<Func<TEntity, bool>> expression)
		{
			return Where(session,expression).Count();
		}
		
	}
}
