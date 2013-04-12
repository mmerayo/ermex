using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Repository
{
	internal interface IReadOnlyRepository<TEntity> 
		where TEntity : ModelBase
	{
		IQueryable<TEntity> FetchAll(ISession session, bool includingOtherComponents = false);
		IQueryable<TEntity> Where(ISession session, Expression<Func<TEntity, bool>> expression);
		TEntity Single(ISession session, int id);
		TEntity Single(ISession session, Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(ISession session, Expression<Func<TEntity, bool>> expression);

		TResult GetMax<TResult>(ISession session, string propertyName);
		bool Any(ISession session, Expression<Func<TEntity, bool>> expression);
	}
}