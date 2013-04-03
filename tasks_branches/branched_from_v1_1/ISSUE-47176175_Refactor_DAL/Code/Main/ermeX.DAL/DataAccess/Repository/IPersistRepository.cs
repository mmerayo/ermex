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
		void Remove(int id);
		void Remove(TEntity entity);
		void Remove(IEnumerable<TEntity> entities);
		void Remove(Expression<Func<TEntity, bool>> expression);
		int Count(Expression<Func<TEntity, bool>> expression);
		void RemoveAll();
	}
}