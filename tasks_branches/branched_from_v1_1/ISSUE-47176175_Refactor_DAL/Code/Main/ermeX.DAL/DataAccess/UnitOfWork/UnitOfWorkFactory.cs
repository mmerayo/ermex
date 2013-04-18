using Common.Logging;
using NHibernate;
using Ninject;
using ermeX.DAL.DataAccess.Providers;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
	internal class UnitOfWorkFactory : IUnitOfWorkFactory
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(UnitOfWorkFactory).FullName);
		private readonly ISessionProvider _sessionFactory;

		[Inject]
		public UnitOfWorkFactory(ISessionProvider sessionProvider)
		{
			_sessionFactory = sessionProvider;
		}

		public IUnitOfWork Create (bool autoCommitWhenDispose=false)
		{
			Logger.DebugFormat("Create. autoCommitWhenDispose={0}",autoCommitWhenDispose);
			var session = _sessionFactory.OpenSession();
			session.FlushMode = FlushMode.Commit;
			return new UnitOfWorkImplementor(this, session,autoCommitWhenDispose);
		}
	}
}