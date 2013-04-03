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
		private ISession _currentSession;

		[Inject]
		public UnitOfWorkFactory(ISessionProvider sessionProvider)
		{
			_sessionFactory = sessionProvider;
		}

		public ISession CurrentSession
		{
			get
			{
				if (_currentSession == null || !_currentSession.IsOpen)
					throw new InvalidOperationException("There is not any current opened session");
				return _currentSession;
			}
			set { _currentSession = value; }
		}

		public IUnitOfWork Create()
		{
			ISession session = CreateSession();
			session.FlushMode = FlushMode.Commit;
			CurrentSession = session;
			return new UnitOfWorkImplementor(this, session);
		}

		private ISession CreateSession()
		{
			return _sessionFactory.OpenSession();
		}

		public void DisposeUnitOfWork(UnitOfWorkImplementor adapter)
		{
			CurrentSession = null;
			UnitOfWork.DisposeUnitOfWork(adapter);
		}

	}
}