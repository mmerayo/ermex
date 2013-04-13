using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate;
using ermeX.DAL.DataAccess.UoW;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.Repository
{
	internal interface IPersistRepository<TEntity> : IReadOnlyRepository<TEntity>
		where TEntity : ModelBase
	{
		bool Save(IUnitOfWork unitofWork, TEntity entity);
		bool Save(IUnitOfWork unitofWork, IEnumerable<TEntity> items);
		void Remove(IUnitOfWork unitofWork, int id);
		void Remove(IUnitOfWork unitofWork, TEntity entity);
		void Remove(IUnitOfWork unitofWork, IEnumerable<TEntity> entities);
		void Remove(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression);
		int Count(IUnitOfWork unitofWork, Expression<Func<TEntity, bool>> expression);
		void RemoveAll(IUnitOfWork unitofWork);
	}
}