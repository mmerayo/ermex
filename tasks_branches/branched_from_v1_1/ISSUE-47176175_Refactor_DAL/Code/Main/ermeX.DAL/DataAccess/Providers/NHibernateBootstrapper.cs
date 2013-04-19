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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Common.Logging;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.SqlCommand;
using NHibernate.Tool.hbm2ddl;
using ermeX;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Mappings;
using Config = NHibernate.Cfg.Configuration;
namespace ermeX.DAL.DataAccess.Providers
{
	//TODO: TO BE INJECTED
    internal static class NHibernateBootstrapper
    {
		private static readonly ILog Logger = LogManager.GetLogger(typeof(NHibernateBootstrapper).FullName);

#if DEBUG
        public class LoggingInterceptor : EmptyInterceptor
        {
            public override SqlString OnPrepareStatement(SqlString sql)
            {
                try
                {
                    Logger.DebugFormat("DomainId: {0} - ThreadId: {1} - SQL: {2}",AppDomain.CurrentDomain.Id, Thread.CurrentThread.ManagedThreadId, sql);
                }
                catch
                {
                }
                return sql;
            }
        }
#endif

        private static NHibernate.Cfg.Configuration _nhConfiguration;
        private static readonly object SyncLock=new object();
        private static IDalSettings _currentSettings;

        public static ISessionFactory BootStrap(IDalSettings settings)
        {
			Logger.Debug("Bootstrap");
            _currentSettings = settings;
            IPersistenceConfigurer concreteDbType = GetConcreteDbType(settings);
            _nhConfiguration = Fluently.Configure()
                .Database(concreteDbType)
                .ExposeConfiguration(GetOrmConfiguration)
                .Mappings(GetMappings)
                .BuildConfiguration();
#if DEBUG
            _nhConfiguration=  _nhConfiguration.SetInterceptor(new LoggingInterceptor());
#endif

            var result = _nhConfiguration.BuildSessionFactory();

//TODO: DBUPDATER based on directory and versions just invoking some sort of DataModelUpdater
#if(DEBUG)
    // UpdateSchema();
#endif
            return result;
        }

       

        private static void GetMappings(MappingConfiguration mappingConfiguration)
        {
            //mappingConfiguration.FluentMappings.AddFromAssemblyOf<MessageDefinitionMap>();

            //Assembly assemblyFromDomain = typeof (AppComponentDataSource).Assembly;

#if EXPORT_MAPPINGS
            var p = PathUtils.GetApplicationFolderPath("MappingsExport");
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            mappingConfiguration.FluentMappings.AddFromAssembly(assemblyFromDomain).ExportTo(p);
#else
            LoadMappings();
            foreach (var mapping in Mappings[_currentSettings.ConfigurationSourceType])
            {
                mappingConfiguration.FluentMappings.Add(mapping);
            }
            //mappingConfiguration.FluentMappings.AddFromAssembly(assemblyFromDomain);
#endif
        }
        private static readonly Dictionary<DbEngineType, List<Type>> Mappings = new Dictionary<DbEngineType, List<Type>>();

        private static void LoadMappings()
        {
            if (!Mappings.ContainsKey(_currentSettings.ConfigurationSourceType))
                lock (SyncLock)
                    if (!Mappings.ContainsKey(_currentSettings.ConfigurationSourceType))
                    {
                        string namespaceStarstWith;
                        switch(_currentSettings.ConfigurationSourceType)
                        {
                            case DbEngineType.SqlServer2008:
                                namespaceStarstWith = "ermeX.DAL.DataAccess.Mappings.SqlServer";
                                break;
                            case DbEngineType.Sqlite:
                            case DbEngineType.SqliteInMemory:
                                namespaceStarstWith = "ermeX.DAL.DataAccess.Mappings.Sqlite";
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        Mappings.Add(_currentSettings.ConfigurationSourceType,new List<Type>(
                            TypesHelper.GetConcreteTypesImplementingGenericType(typeof (ClassMap<>),
                                                                                new[]
                                                                                    {typeof (AppComponentMap).Assembly},
                                                                                namespaceStarstWith)));
                    }
        }

        private static void GetOrmConfiguration(Config cfg)
        {
            //TODO: Add caching and others
            cfg.SetProperty("connection.release_mode", "on_close");
#if DEBUG
            cfg.SetProperty("nhibernate.show_sql", "true");
#endif   
        }

        private static IPersistenceConfigurer GetConcreteDbType(IDalSettings settings)
        {
            IPersistenceConfigurer result;
            switch (settings.ConfigurationSourceType)
            {
                case DbEngineType.SqlServer2008:
                    result = MsSqlConfiguration.MsSql2008.ConnectionString(settings.ConfigurationConnectionString);
                    break;
                case DbEngineType.Sqlite:
                case DbEngineType.SqliteInMemory:
                    result = SQLiteConfiguration.Standard.ConnectionString(settings.ConfigurationConnectionString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        public static void UpdateSchema()
        {
			Logger.Debug("UpdateSchema");
            new SchemaUpdate(_nhConfiguration)
                .Execute(false, true);
        }
    }
}