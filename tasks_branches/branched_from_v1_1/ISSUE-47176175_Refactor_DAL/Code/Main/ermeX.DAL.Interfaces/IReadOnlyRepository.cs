using System;
using System.Linq;
using System.Linq.Expressions;
using ermeX.Entities.Base;

namespace ermeX.DAL.Interfaces
{
	internal interface IReadOnlyRepository<TEntity> 
		where TEntity : ModelBase
	{
		IQueryable<TEntity> FetchAll(bool includingOtherComponents=false);
		IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression);
		TEntity Single(int id);
		TEntity Single(Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression);

		TResult GetMax<TResult>(string propertyName);
	}
}