using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Repository
{
	internal interface IPersistRepository<TEntity> where TEntity : ModelBase
	{
		bool Save(TEntity entity);
		bool Save(IEnumerable<TEntity> items);
		bool Remove(int id);
		bool Remove(TEntity entity);
		bool Remove(IEnumerable<TEntity> entities);
		bool Remove(Expression<Func<TEntity, bool>> expression);
	}
}