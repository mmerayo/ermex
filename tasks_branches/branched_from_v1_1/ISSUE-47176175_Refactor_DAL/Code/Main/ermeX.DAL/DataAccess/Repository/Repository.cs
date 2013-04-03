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
	internal sealed class Repository<TEntity> : IPersistRepository<TEntity>
		 where TEntity : ModelBase
	{
		private readonly Guid _localComponentId;
		private readonly IUnitOfWorkFactory _factory;

		[Inject]
		public Repository(IComponentSettings settings, 
			IUnitOfWorkFactory factory)
		{
			if (settings.ComponentId==Guid.Empty)
				throw new ArgumentEmptyException("localComponentId");
			if (factory == null) throw new ArgumentNullException("factory");
			_localComponentId = settings.ComponentId;
			_factory = factory;
		}

		private Expression<Func<TEntity, bool>> ModelToConcreteExpressionConversion(Expression<Func<object, bool>> expression)
		{
			return obj => expression.Compile().Invoke(obj);
		}

		public bool Save(TEntity entity)
		{
			if (entity.ComponentOwner == Guid.Empty)
				throw new ArgumentEmptyException("entity.ComponentOwner");

			if (!CanSave(entity))
				return false;


			if (entity.ComponentOwner == _localComponentId)
				entity.Version = DateTime.UtcNow.Ticks; //Keeps the version of the last updater

			entity.ComponentOwner = _localComponentId;

			_factory.CurrentSession.SaveOrUpdate(entity);
			return true;
		}

		private bool CanSave(TEntity entity)
		{
			var item = SingleOrDefault(ModelToConcreteExpressionConversion(entity.FindByBizKey));
			return item == null || item.Version <= entity.Version; //Can save if it didnt exist or the version is newer
		}
		
		public bool Save(IEnumerable<TEntity> items)
		{
			foreach (TEntity item in items)
				Save(item);
			return true;
		}

		public void RemoveAll()
		{
			var fetchAll = FetchAll();
			Remove(fetchAll);
		}

		public void Remove(int id)
		{
			var toRemove = Single(id);
			Remove(toRemove);
		}

		public void Remove(TEntity entity)
		{
			_factory.CurrentSession.Delete(entity);
		}

		public void Remove(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
				Remove(entity);
		}

		public void Remove(Expression<Func<TEntity, bool>> expression)
		{
			IQueryable<TEntity> queryable = Where(expression);
			Remove(queryable);
		}

		public IQueryable<TEntity> FetchAll(bool includingOtherComponents = false)
		{
			var absolute = _factory.CurrentSession.Query<TEntity>();
			if(!includingOtherComponents)
				return absolute.Where(IsLocalPredicate);
			return absolute;
		}

		public TEntity Single(int id)
		{
			var result = _factory.CurrentSession.Get<TEntity>(id);
			if (result.ComponentOwner != _localComponentId)
				throw new InvalidOperationException("The id is owned by another component");
			return result;
		}

		public TEntity Single(Expression<Func<TEntity, bool>> expression)
		{
			return Where(expression).Single();
		}

		public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression)
		{
			return Where(expression).SingleOrDefault();
		}

		public TResult GetMax<TResult>(string propertyName)
		{
			return
				_factory.CurrentSession.QueryOver<TEntity>()
				        .Where(IsLocalPredicate)
				        .Select(Projections.Max(propertyName))
				        .SingleOrDefault<TResult>();

		}

		public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression)
		{
			return _factory.CurrentSession.Query<TEntity>().Where(IsLocalPredicate).Where(expression); //TODO: IMPROVE USING AND BETWEEN BOTH EXPRESSIONS
		}

		private Expression<Func<TEntity, bool>> IsLocalPredicate
		{
			get { return x=>x.ComponentOwner==_localComponentId; }
		}

		public int Count(Expression<Func<TEntity, bool>> expression)
		{
			return Where(expression).Count();
		}

		
	}
}
