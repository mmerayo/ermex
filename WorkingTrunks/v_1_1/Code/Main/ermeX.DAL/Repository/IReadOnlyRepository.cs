using System;
using System.Linq;
using System.Linq.Expressions;
using ermeX.DAL.UnitOfWork;
using ermeX.Models.Base;

namespace ermeX.DAL.Repository
{
	internal interface IReadOnlyRepository<TEntity>
		where TEntity : ModelBase
	{
		//implicit unit of work of one operation
		IQueryable<TEntity> FetchAll(bool includingOtherComponents = false);
		IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression);
		TEntity Single(int id);
		TEntity Single(Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(int id);
		TResult GetMax<TResult>(string propertyName);
		bool Any(Expression<Func<TEntity, bool>> expression);
		int Count(Expression<Func<TEntity, bool>> expression);
		bool Any();
		int Count();

		//in specified unit of work
		IQueryable<TEntity> FetchAll(IUnitOfWork unitOfWork, bool includingOtherComponents = false);
		IQueryable<TEntity> Where(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression);
		TEntity Single(IUnitOfWork unitOfWork, int id);
		TEntity Single(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression);
		TEntity SingleOrDefault(IUnitOfWork unitOfWork, int id);
		TResult GetMax<TResult>(IUnitOfWork unitOfWork, string propertyName);
		bool Any(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression);
		int Count(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> expression);
		bool Any(IUnitOfWork unitOfWork);
		int Count(IUnitOfWork unitOfWork);
	}
}