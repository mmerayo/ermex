using NHibernate;

namespace ermeX.DAL.Interfaces.UnitOfWork
{
    internal interface IUnitOfWorkFactory
    {
        NHibernate.Cfg.Configuration Configuration { get; }
        ISessionFactory SessionFactory { get; }
        ISession CurrentSession { get; set; }

        IUnitOfWork Create();
        void DisposeUnitOfWork(IUnitOfWorkImplementor adapter);
    }
}