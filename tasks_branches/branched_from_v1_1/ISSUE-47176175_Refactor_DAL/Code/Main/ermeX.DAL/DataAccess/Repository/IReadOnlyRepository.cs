using System;
using System.Linq;
using System.Linq.Expressions;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Repository
{
	internal interface IReadOnlyRepository<TEntity> where TEntity : ModelBase
	{
		IQueryable<TEntity> FetchAll();
		TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression);
		IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression);
		TEntity Single(int id);
	}
}