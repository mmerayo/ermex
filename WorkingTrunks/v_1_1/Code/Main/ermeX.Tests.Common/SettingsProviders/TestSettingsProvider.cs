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
using System.Collections.Specialized;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Common;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Networking;

namespace ermeX.Tests.Common.SettingsProviders
{
    internal static class TestSettingsProvider
    {

        private static readonly Dictionary<DbEngineType, ITestDbEngine> CurrentDbs = new Dictionary<DbEngineType, ITestDbEngine>(typeof(DbEngineType).GetEnumValues().Length);
        private static readonly object SyncLock=new object();

        public static IDalSettings GetDataAccessSettingsSource(DbEngineType type,
                                                                          List<DataSchemaType> schemasToApply)
        {
            var result = new DataAccessSettings
                             {
                                 ConfigurationSourceType = type,
                                 SchemasApplied = schemasToApply,
                                 ConfigurationConnectionString = GetConnString(type)
                             };

            return result;
        }

        public static string GetConnString(DbEngineType engineType)
        {
            if (!CurrentDbs.ContainsKey(engineType) || CurrentDbs[engineType] == null)
                lock (SyncLock)
                {
                    if (!CurrentDbs.ContainsKey(engineType))
                        CurrentDbs[engineType] = null;
                    ITestDbEngine dbEngine;
                    if (CurrentDbs[engineType] == null)
                    {
                        switch (engineType)
                        {
                            case DbEngineType.SqlServer2008:
                                dbEngine = new SqlServerDbEngine(Guid.NewGuid().ToString());
                                break;
                            case DbEngineType.Sqlite:
                                dbEngine = new SqliteDbEngine(Guid.NewGuid().ToString());
                                break;
                            case DbEngineType.SqliteInMemory:
                                dbEngine = new SqliteDbEngine(Guid.NewGuid().ToString(),true);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("engineType");
                        }
                        dbEngine.CreateDatabase();
                        CurrentDbs[engineType] = dbEngine;
                    }
                }
            return CurrentDbs[engineType].GetConnectionString();
        }

        public static Configurer GetServiceLayerSettingsSource(Guid componentId, DbEngineType engine,
                                                                  bool devLoggingOn=false)
        {

            var result = Configurer.Configure(componentId).SendMessagesExpirationTime(TimeSpan.FromMinutes(5))
                .ListeningToTcpPort(new TestPort(20000));
            switch(engine)
            {
                case DbEngineType.SqlServer2008:
                    result = result.SetSqlServerDb(GetConnString(engine));
                    break;
                case DbEngineType.Sqlite:
                    result = result.SetSqliteDb(GetConnString(engine));
                    break;
                case DbEngineType.SqliteInMemory:
                    result = result.SetInMemoryDb(GetConnString(engine));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("engine");
            }

                
            if(devLoggingOn)
                result.WithDevelopmentLoggingOn();

            return result;
        }

        public static ITestSettings GetClientConfigurationSettingsSource()
        {
            //TODO: to constructor
            var result = new ClientSettings
                             {
                                 ComponentId = Guid.NewGuid()
                                 ,SendExpiringTime = TimeSpan.FromDays(1)
                                 //TODO: add them on demand
                             };
            return result;
        }

        #region Db Creation

        public static void DropDatabase(DbEngineType engineType)
        {
            lock (SyncLock)
            {
                if (!CurrentDbs.ContainsKey(engineType) || CurrentDbs[engineType] == null)
                    return;

                CurrentDbs[engineType].DropDatabase();
                CurrentDbs.Remove(engineType);
            }
        }


        #endregion

        #region Nested type: ClientSettings

        public class ClientSettings : ITestSettings
        {
            private int _maxMessageKbBeforeChunking = 256;
            public int CacheExpirationSeconds { get; set; }

            public NetworkingMode NetworkingMode { get; set; }

            public FriendComponentData FriendComponent { get; set; }

            public Guid ComponentId { get; set; }


            public IEsbManager BusManager
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IMessagePublisherDispatcherStrategy MessageDispatcher
            {
                get { throw new NotImplementedException(); }
            }

           

            public Type ConfigurationManagerType { get; set; }

            public bool DevLoggingActive
            {
                get { return true; }
            }

            public bool InternalDiagnosticsActive { get; set; }

           
            public TimeSpan SendExpiringTime { get; set; }

            public int InternalWinRpcPort { get; set; }

           

            public int MaxDelayDueToLatencySeconds
            {
                get { return 20; }
            }


            public int MaxMessageKbBeforeChunking
            {
                get { return _maxMessageKbBeforeChunking; }
                set { _maxMessageKbBeforeChunking = value; }
            }

            public ushort TcpPort
            {
                get { throw new NotImplementedException(); }
            }
        }

        #endregion

        #region Nested type: DataAccessSettings

        public class DataAccessSettings : IDalSettings
        {
            public string ConfigurationConnectionString { get; set; }

            public DbEngineType ConfigurationSourceType { get; set; }
            public DbConnection InMemoryConnection { get; set; }

            public IList<DataSchemaType> SchemasApplied { get; set; }


        }

        #endregion

        public static void DropDatabases()
        {
            lock(SyncLock)
            {
                var keyCollection = new List<DbEngineType>(CurrentDbs.Keys);
                foreach (var value in keyCollection)
                {
                    DropDatabase(value);
                }
            }
        }
    }
}