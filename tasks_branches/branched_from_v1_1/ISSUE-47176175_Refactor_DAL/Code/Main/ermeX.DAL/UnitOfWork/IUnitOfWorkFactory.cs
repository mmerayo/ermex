namespace ermeX.DAL.UnitOfWork
{
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create(bool autoCommitWhenDispose=false);
	}
}