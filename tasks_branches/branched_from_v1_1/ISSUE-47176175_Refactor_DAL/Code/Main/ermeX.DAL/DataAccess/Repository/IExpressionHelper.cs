using System;
using System.Linq.Expressions;

namespace ermeX.DAL.DataAccess.Repository
{
	internal interface IExpressionHelper<TModel>
	{
		Expression<Func<TModel, bool>> GetFindByBizKey(TModel e);
	}
}