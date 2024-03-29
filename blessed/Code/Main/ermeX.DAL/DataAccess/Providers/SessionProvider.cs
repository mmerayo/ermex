// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
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