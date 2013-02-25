using System;
using System.IO;
using System.Xml;
using NHibernate;
using ermeX.DAL.Interfaces.UnitOfWork;

namespace ermeX.DAL.DataAccess.UnitOfWork
{
    internal class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private const string Default_HibernateConfig = "hibernate.cfg.xml";

        private static ISession _currentSession;
        private ISessionFactory _sessionFactory;
        private NHibernate.Cfg.Configuration _configuration;

        internal UnitOfWorkFactory()
        { }

        public IUnitOfWork Create()
        {
            ISession session = CreateSession();
            session.FlushMode = FlushMode.Commit;
            _currentSession = session;
            return new UnitOfWorkImplementor(this, session);
        }

        public NHibernate.Cfg.Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new NHibernate.Cfg.Configuration();
                    string hibernateConfig = Default_HibernateConfig;
                    //if not rooted, assume path from base directory
                    if (Path.IsPathRooted(hibernateConfig) == false)
                        hibernateConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hibernateConfig);
                    if (File.Exists(hibernateConfig))
                        _configuration.Configure(new XmlTextReader(hibernateConfig));
                }
                return _configuration;
            }
        }

        public ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                    _sessionFactory = Configuration.BuildSessionFactory();
                return _sessionFactory;
            }
        }

        public ISession CurrentSession
        {
            get
            {
                if (_currentSession == null)
                    throw new InvalidOperationException("You are not in a unit of work.");
                return _currentSession;
            }
            set { _currentSession = value; }
        }

        public void DisposeUnitOfWork(IUnitOfWorkImplementor adapter)
        {
            CurrentSession = null;
            UnitOfWork.DisposeUnitOfWork(adapter);
        }

        private ISession CreateSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}