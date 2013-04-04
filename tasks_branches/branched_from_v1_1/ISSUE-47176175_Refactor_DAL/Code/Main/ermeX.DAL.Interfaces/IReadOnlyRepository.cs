using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ermeX.Entities.Base;
using ermeX.Entities.Entities;

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
		bool Any(Expression<Func<TEntity, bool>> expression);
	}
}