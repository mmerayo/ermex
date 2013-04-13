using NHibernate;

namespace ermeX.DAL.DataAccess.UoW
{
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create(bool autoCommitWhenDispose=false);
	}
}