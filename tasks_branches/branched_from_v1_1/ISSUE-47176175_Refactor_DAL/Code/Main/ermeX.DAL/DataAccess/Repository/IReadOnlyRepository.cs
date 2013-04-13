using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Repository
{
	internal interface IReadOnlyRepository<TEntity> 
		where TEntity : ModelBase
	{
		IQueryable<TEntity> FetchAll(IUnitOfWork unitofWork, bool includingOtherComponents = false);
		IQueryable<TEntity> Where(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression);
		TEntity Single(IUnitOfWork unitofWork, int id);
		TEntity Single(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(IUnitOfWork unitofWork, int id);
		TResult GetMax<TResult>(IUnitOfWork unitofWork, string propertyName);
		bool Any(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression);
	}
}