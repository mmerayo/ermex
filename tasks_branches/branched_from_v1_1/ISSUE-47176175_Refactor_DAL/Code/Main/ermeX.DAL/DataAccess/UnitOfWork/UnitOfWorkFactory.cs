using System;
using NHibernate;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Providers;

namespace ermeX.DAL.DataAccess.UoW
{
	internal class UnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionProvider _sessionFactory;

		[Inject]
		public UnitOfWorkFactory(ISessionProvider sessionProvider)
		{
			_sessionFactory = sessionProvider;
		}

		public IUnitOfWork Create()
		{
			var session = _sessionFactory.OpenSession();
			session.FlushMode = FlushMode.Commit;
			return new UnitOfWorkImplementor(this, session);
		}
	}
}