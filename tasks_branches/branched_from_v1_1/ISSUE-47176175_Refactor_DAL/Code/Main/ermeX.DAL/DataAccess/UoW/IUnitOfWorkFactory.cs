using NHibernate;

namespace ermeX.DAL.DataAccess.UoW
{
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create();
		ISession CurrentSession { get; set; }
		void DisposeUnitOfWork(UnitOfWorkImplementor adapter);
	}
}