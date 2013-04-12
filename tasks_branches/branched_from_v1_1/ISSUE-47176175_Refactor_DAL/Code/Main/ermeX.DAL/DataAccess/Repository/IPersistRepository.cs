using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Repository
{
	internal interface IPersistRepository<TEntity> : IReadOnlyRepository<TEntity>
		where TEntity : ModelBase
	{
		bool Save(ISession session, TEntity entity);
		bool Save(ISession session, IEnumerable<TEntity> items);
		void Remove(ISession session, int id);
		void Remove(ISession session, TEntity entity);
		void Remove(ISession session, IEnumerable<TEntity> entities);
		void Remove(ISession session, Expression<Func<TEntity, bool>> expression);
		int Count(ISession session, Expression<Func<TEntity, bool>> expression);
		void RemoveAll(ISession session);
	}
}