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
		//implicit unit of work of one operation
		bool Save(TEntity entity);
		bool Save(IEnumerable<TEntity> items);
		void Remove(int id);
		void Remove(TEntity entity);
		void Remove(IEnumerable<TEntity> entities);
		void Remove(Expression<Func<TEntity, bool>> expression);
		void RemoveAll();


		bool Save(IUnitOfWork unitOfWork, TEntity entity);
		bool Save(IUnitOfWork unitOfWork, IEnumerable<TEntity> items);
		void Remove(IUnitOfWork unitOfWork, int id);
		void Remove(IUnitOfWork unitOfWork, TEntity entity);
		void Remove(IUnitOfWork unitOfWork, IEnumerable<TEntity> entities);
		void Remove(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression);
		void RemoveAll(IUnitOfWork unitOfWork);
	}
}