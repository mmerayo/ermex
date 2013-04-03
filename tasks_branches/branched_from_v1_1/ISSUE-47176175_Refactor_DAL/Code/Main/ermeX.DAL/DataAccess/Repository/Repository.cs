using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using Remotion.Linq.Utilities;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Repository
{
	internal abstract class Repository<TEntity> : IPersistRepository<TEntity>,
		IReadOnlyRepository<TEntity> where TEntity : ModelBase
	{
		private readonly Guid _localComponentId;
		private readonly IUnitOfWorkFactory _factory;

		//Expresion to find an entity other than the localkey
		protected abstract Expression<Func<TEntity, bool>> FindByBizKey { get; } //TODO: REMOVE see beforeupdating

		protected Repository(Guid localComponentId, 
			IUnitOfWorkFactory factory)
		{
			if (localComponentId==Guid.Empty)
				throw new ArgumentEmptyException("localComponentId");
			if (factory == null) throw new ArgumentNullException("factory");
			_localComponentId = localComponentId;
			_factory = factory;
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
			var item = SingleOrDefault(FindByBizKey);
			return item == null || item.Version <= entity.Version; //Can save if is null or the version is newer
		}

		public bool Save(IEnumerable<TEntity> items)
		{
			foreach (TEntity item in items)
			{
				Save(item);
			}
			return true;
		}

		public bool Remove(int id)
		{
			var toRemove = Single(id);
			Remove(toRemove);
			return true;
		}

		public bool Remove(TEntity entity)
		{
			_factory.CurrentSession.Delete(entity);
			return true;
		}

		public bool Remove(IEnumerable<TEntity> entities)
		{
			foreach (var entity in entities)
				Remove(entity);
			return true;
		}

		public bool Remove(Expression<Func<TEntity, bool>> expression)
		{
			IQueryable<TEntity> queryable = Where(expression);
			Remove(queryable);
			return true;
		}

		public IQueryable<TEntity> FetchAll()
		{
			return _factory.CurrentSession.Query<TEntity>();
		}

		public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression)
		{
			return Where(expression).SingleOrDefault();
		}

		public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression)
		{
			return _factory.CurrentSession.Query<TEntity>().Where(expression);
		}

		public TEntity Single(int id)
		{
			return _factory.CurrentSession.Get<TEntity>(id);
		}
	}
}
