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
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using ermeX.Common;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;


namespace ermeX.Tests.Acceptance.Dummy
{
    internal class TestComponent : MarshalByRefObject, IDisposable
    {
        private static readonly List<AppDomain> LoadedDomains = new List<AppDomain>();

        public static TestComponent GetComponent(Guid componentId,bool useCurrentAppDomain = false)
        {
            TestComponent result;
            if (!useCurrentAppDomain)
            {
                string pathToTheDll = String.Format("{0}\\ermeX.Tests.dll",
                                                    PathUtils.GetPath(Assembly.GetExecutingAssembly().CodeBase));
                AppDomain myDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString().Replace('-', '_'), null,
                                                            new AppDomainSetup() { ApplicationBase = Path.GetDirectoryName(pathToTheDll) });
                LoadedDomains.Add(myDomain);

                var wrappedResult = myDomain.CreateInstanceFrom(pathToTheDll, typeof(TestComponent).FullName);

                result = (TestComponent)wrappedResult.Unwrap();
                result.SetConsoleOut(Console.Out);

            }
            else
            {
                result= new TestComponent();
            }
            result.ComponentId = componentId;
            return result;

        }

        public static TestComponent GetComponent(bool useCurrentAppDomain = false)
        {
            return GetComponent(Guid.NewGuid(), useCurrentAppDomain);
        }

        public static void DisposeDomains()
        {
            while (LoadedDomains.Count > 0)
            {
                try
                {
                    AppDomain.Unload(LoadedDomains[0]);
                }
                catch (CannotUnloadAppDomainException)
                {
                }
                finally
                {
                    LoadedDomains.RemoveAt(0);
                }
            }
        }

        [SecurityPermissionAttribute(SecurityAction.Demand,
            Flags = SecurityPermissionFlag.Infrastructure)]
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        private void SetConsoleOut(TextWriter outStream)
        {
            Console.SetOut(outStream);
        }



        public void Start(DbEngineType engineType, string dbConnString, ushort listeningPort)
        {
            var cfg = Configurer.Configure(ComponentId)
                .ListeningToTcpPort(listeningPort);
            switch (engineType)
            {
                case DbEngineType.SqlServer2008:
                    cfg = cfg.SetSqlServerDb(dbConnString);
                    break;
                case DbEngineType.Sqlite:
                    cfg = cfg.SetSqliteDb(dbConnString);
                    break;
                case DbEngineType.SqliteInMemory:
                    cfg = cfg.SetInMemoryDb(dbConnString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("engineType");
            }
            cfg.WithDevelopmentLoggingOn();

			WorldGate.Reset();
            WorldGate.ConfigureAndStart(cfg);

        }

        public void Start(DbEngineType engineType,string dbConnString, ushort listeningPort, Guid joinToComponentId, ushort jointToPort)
        {

            var cfg = Configurer.Configure(ComponentId)
                .ListeningToTcpPort(listeningPort)
                .RequestJoinTo(Networking.GetLocalhostIp(), jointToPort, joinToComponentId);
            switch (engineType)
            {
                case DbEngineType.SqlServer2008:
                    cfg = cfg.SetSqlServerDb(dbConnString);
                    break;
                case DbEngineType.Sqlite:
                    cfg = cfg.SetSqliteDb(dbConnString);
                    break;
                case DbEngineType.SqliteInMemory:
                    cfg = cfg.SetInMemoryDb(dbConnString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("engineType");
            }
            cfg.WithDevelopmentLoggingOn();

            WorldGate.ConfigureAndStart(cfg);
        }

        public void Start(DbEngineType engineType, Guid componentId, string dbConnString, ushort listeningPort, Guid joinToComponentId,
                          ushort jointToPort)
        {
            ComponentId = componentId;
            Start(engineType, dbConnString, listeningPort, joinToComponentId, jointToPort);
        }

        public void Start(DbEngineType engineType, Guid componentId, string dbConnString, ushort listeningPort)
        {
            ComponentId = componentId;
            Start(engineType,dbConnString, listeningPort);
        }

        public Guid ComponentId { get; set; }

        internal TestService.TrackerData Tracker
        {
            get
            {
                TestService.Refresh();
                return TestService.Tracker;
            }
        }

        public void Publish(object message)
        {
            WorldGate.Publish(message);
        }

        public TResult Suscribe<TResult>(Type handlerType)
        {
            return WorldGate.Suscribe<TResult>(handlerType);
        }

        public void RegisterService(int numOfRequestsToReceive, AutoResetEvent registeredEvent,
                                    AutoResetEvent requestsHandler)
        {
            RegisterService<ITestService1>(numOfRequestsToReceive, registeredEvent, requestsHandler);
        }

        public void RegisterService<TService>(int numOfRequestsToReceive, AutoResetEvent registeredEvent,
                                              AutoResetEvent requestsHandler) where TService : IService
        {
            TestService.Reset();
            TestService.NotifyWhenRequestsReceived(numOfRequestsToReceive, requestsHandler);
            WorldGate.RegisterService<TService>(typeof (TestService));
            registeredEvent.Set();
        }

        public TService GetServiceProxy<TService>() where TService : IService
        {
            return WorldGate.GetServiceProxy<TService>();
        }


        public void Dispose()
        {
            WorldGate.Reset();
        }

        public void Reset()
        {
            WorldGate.Reset();
        }
    }
}