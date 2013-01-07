// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.SqlCommand;
using NHibernate.Tool.hbm2ddl;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.Mappings;

namespace ermeX.DAL.DataAccess.Providers
{
    internal static class NHibernateBootstrapper
    {
#if DEBUG
        public class LoggingInterceptor : EmptyInterceptor
        {
            public override SqlString OnPrepareStatement(SqlString sql)
            {
                try
                {
                    Console.WriteLine(sql);
                }
                catch
                {
                }
                return sql;
            }
        }
#endif

        private static Configuration _nhConfiguration;
        private static readonly object SyncLock=new object();
        private static IDalSettings _currentSettings;

        public static ISessionFactory BootStrap(IDalSettings settings)
        {
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

        private static void GetOrmConfiguration(Configuration cfg)
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
            new SchemaUpdate(_nhConfiguration)
                .Execute(false, true);
        }
    }
}