namespace ermeX.DAL.DataAccess.UnitOfWork
{
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create(bool autoCommitWhenDispose=false);
	}
}