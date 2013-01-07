// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Data.SQLite;
using NHibernate;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;

namespace ermeX.DAL.DataAccess.Providers
{
    internal sealed class SessionProvider : ISessionProvider
    {
        private readonly IDalSettings _settings;

        private volatile  ISessionFactory _sessionFactory;
        private static SQLiteConnection _inMemoryDb;
        //private volatile Dictionary<DbEngineType, ISessionFactory> _sessionFactories = new Dictionary<DbEngineType, ISessionFactory>(Enum.GetValues(typeof(DbEngineType)).Length); 

        

        [Inject]
        public SessionProvider(IDalSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            _settings = settings;
        }

        private ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    lock (this)
                    {
                        if (_sessionFactory == null)
                        {
                            _sessionFactory = NHibernateBootstrapper.BootStrap(_settings);
                        }
                    }
                }
                return _sessionFactory;
            }
        }

        
        public ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        public static void SetInMemoryDb(string connectionString)
        {
            if (_inMemoryDb != null && _inMemoryDb.ConnectionString == connectionString)
                return;
            _inMemoryDb = new SQLiteConnection(connectionString);//TODO: DIsPOSE? not yet as it is in memory

            _inMemoryDb.Open();
        }
    }
}