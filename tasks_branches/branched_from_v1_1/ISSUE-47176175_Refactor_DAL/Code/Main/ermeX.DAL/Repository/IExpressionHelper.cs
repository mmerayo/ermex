using System;
using System.Linq.Expressions;

namespace ermeX.DAL.Repository
{
	internal interface IExpressionHelper<TModel>
	{
		Expression<Func<TModel, bool>> GetFindByBizKey(TModel e);
	}
}