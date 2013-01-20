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
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;


using ermeX.Interfaces;
using ermeX.NonMerged;

namespace ermeX.ConfigurationManagement
{
    /// <summary>
    ///   Holds the ermeX component configuration
    /// </summary>
    public partial class Configuration
    {
        static Configuration()
        {
            ResolveUnmerged.Prepare();
        }

        private readonly RealConfigure _configuration;

        private Configuration(Guid componentId)
        {
            if (componentId == Guid.Empty)
                throw new ArgumentException("the value cannot be empty", "componentId");

            //internal fields & default values
            _configuration = new RealConfigure
                                 {
                                     //ComponentSettings
                                     ComponentId = componentId,
                                     CacheExpirationSeconds = new TimeSpan(1, 0, 0).Seconds,
                                     NetworkingMode = NetworkingMode.Anarquik,
                                     FriendComponent = null,
                                     //dataaccess
                                     ConfigurationSourceType = DbEngineType.Sqlite,
                                     ConfigurationConnectionString = String.Empty,
                                     //TODO: SET DEFAULT CONNECTION STRING for SQLLite and remove it when changing dbtype
                                     ConfigurationManagerType = null,
                                     //IBusSettings
                                     BusManager = null,
                                     MessageDispatcher = null,
                                     //IClientconfigurationSettings
                                     
                                     //TODO: MUST CHANGE FOR TESTS 
                                     SendExpiringTime = TimeSpan.FromDays(31),
                                     MaxDelayDueToLatencySeconds = 60,
                                     MaxMessageKbBeforeChunking = 1024,
                                     //3MB
                                     TcpPort = 8135,

                                     DevLoggingActive= false
                                 };
        }



        internal TConfigModule GetSettings<TConfigModule>() where TConfigModule : class
        {
            return _configuration as TConfigModule;
        }

        public static Configuration Configure(Guid componentId)
        {
            if (componentId == Guid.Empty)
                throw new ArgumentException("the value cannot be empty", "componentId");
            return new Configuration(componentId);
        }


        public Configuration SetSqlServerDb(string dbConnString)
        {
            _configuration.ConfigurationSourceType = DbEngineType.SqlServer2008;
            _configuration.ConfigurationConnectionString = dbConnString;
            return this;
        }

        public Configuration SetSqliteDb(string dbConnString)
        {
            _configuration.ConfigurationSourceType = DbEngineType.Sqlite;
            _configuration.ConfigurationConnectionString = dbConnString;
            return this;
        }

        /// <summary>
        /// This configuration sets an inmemory db.
        /// </summary>
        /// <returns></returns>
        /// <remarks>No data is persisted, use only if message delivery between sessions is not mandatory</remarks>
        public Configuration SetInMemoryDb()
        {
            return
                SetInMemoryDb(string.Format("FullUri=file:{0}?mode=memory&cache=shared;Version=3;BinaryGuid=False",
                                            Guid.NewGuid().ToString().Replace('-', '_')));

        }

        internal Configuration SetInMemoryDb(string connectionString) //for tests
        {
            _configuration.ConfigurationSourceType = DbEngineType.SqliteInMemory;
            _configuration.ConfigurationConnectionString = connectionString;

            return this;
        }



        public Configuration SendMessagesExpirationTime(TimeSpan expireAfter)
        {
            _configuration.SendExpiringTime = expireAfter;
            return this;
        }

        public Configuration ListeningToTcpPort(ushort tcpPort)
        {
            if (tcpPort <= 1023)
                throw new ArgumentOutOfRangeException("tcpPort",
                                                      "The selected port cannot be in the range of the well-known ports");


            _configuration.TcpPort = tcpPort;
            return this;
        }

        /// <summary>
        ///   Component that will provide the rest of the components, services and suscriptions
        /// </summary>
        /// <param name="ermeXComponent"> </param>
        /// <param name="componentId"> </param>
        /// <returns> </returns>
        public Configuration RequestJoinTo(IPEndPoint ermeXComponent, Guid componentId)
        {
            if (ermeXComponent == null) throw new ArgumentNullException("ermeXComponent");

            _configuration.FriendComponent = new FriendComponentData
                                                 {
                                                     ComponentId = componentId,
                                                     Endpoint = ermeXComponent
                                                 };
            return this;
        }

        public Configuration RequestJoinTo(string remoteIp, int remotePort, Guid componentId)
        {
            var strings = remoteIp.Split('.');
            var bytes = new byte[strings.Length];

            for (int index = 0; index < strings.Length; index++)
            {
                var s = strings[index];
                bytes[index] = byte.Parse(s);
            }
            var ipAddress = new IPAddress(bytes);
            return RequestJoinTo(new IPEndPoint(ipAddress, remotePort), componentId);
        }

        [Conditional("DEBUG")]
        public void WithDevelopmentLoggingOn()
        {
            _configuration.DevLoggingActive = true;
        }



        public Configuration DiscoverSubscriptors(Assembly[] assemblies, Type[] suscriberTypesToExclude)
        {
            _configuration.SearchMessageSuscriptionAssemblies = assemblies;
            _configuration.SearchMessageSuscriptionExcludeTypes = suscriberTypesToExclude;
            return this;
        }

        public Configuration DiscoverServicesToPublish(Assembly[] assemblies, Type[] suscriberTypesToExclude)
        {
            _configuration.SearchServicesToPublishAssemblies = assemblies;
            _configuration.SearchServicesToPublishExcludeTypes = suscriberTypesToExclude;
            return this;
        }

        internal IEnumerable<DiscoveredSubscription> GetDiscoveredSubscriptions()
        {
            if(_configuration.SearchMessageSuscriptionAssemblies==null)
                return new List<DiscoveredSubscription>();

            var preTypes = TypesHelper.GetConcreteTypesImplementingGenericType(typeof (IHandleMessages<>), _configuration.SearchMessageSuscriptionAssemblies).ToList();
            List<Type> handlerTypes=new List<Type>(preTypes.Count);

            foreach (var type in preTypes)
            {
                var @interfaces = TypesHelper.GetGenericInterfaces(typeof(IHandleMessages<>), type);
                if (@interfaces == null|| @interfaces.Length==0)
                    throw new ArgumentException("handlerType must implement IHandleMessages");
                if (type.IsAbstract)
                    continue;
                handlerTypes.Add(type);
            }
            if (_configuration.SearchMessageSuscriptionExcludeTypes != null)
            {
                int removed=handlerTypes.RemoveAll(x => _configuration.SearchMessageSuscriptionExcludeTypes.Contains(x));
                Debug.Assert(_configuration.SearchMessageSuscriptionExcludeTypes == null ||
                             _configuration.SearchMessageSuscriptionExcludeTypes.Length == 0 || removed > 0);
            }

            if (_configuration.SearchMessageSuscriptionExcludeTypes != null)
            {
                foreach (
                    var searchMessageSuscriptionExcludeType in
                        _configuration.SearchMessageSuscriptionExcludeTypes.Where(
                            searchMessageSuscriptionExcludeType =>
                            handlerTypes.Contains(searchMessageSuscriptionExcludeType)))
                    handlerTypes.Remove(searchMessageSuscriptionExcludeType);
            }
            //removes repeated handlers 
            handlerTypes= handlerTypes.Select(x=>x).Distinct().ToList();

            var result = new List<DiscoveredSubscription>(handlerTypes.Count);

            foreach (var handlerType in handlerTypes)
            {
                var @interfaces = TypesHelper.GetGenericInterfaces(typeof (IHandleMessages<>), handlerType);
                foreach (var @interface in @interfaces)
                {
                    var discoveredSubscription = new DiscoveredSubscription()
                                                     {
                                                         InterfaceType=@interface,
                                                         HandlerType = handlerType,
                                                         MessageType =@interface.GetGenericArguments()[0]
                                                     };
                    result.Add(discoveredSubscription);
                }
            }

            return result;
        }


        internal IEnumerable<DiscoveredService> GetDiscoveredServices()
        {
            if (_configuration.SearchServicesToPublishAssemblies== null)
                return new List<DiscoveredService>();

            var types = TypesHelper.GetTypesImplementing(typeof(IService), _configuration.SearchServicesToPublishAssemblies).ToList();
           
            var result = new List<DiscoveredService>(types.Count);
            foreach (var type in types)
            {
                IEnumerable<Type> svcTypes = TypesHelper.GetInterfacesImplementing<IService>(type);

                foreach (var svcType in svcTypes)
                {
                    if (_configuration.SearchServicesToPublishExcludeTypes==null ||
                        !_configuration.SearchServicesToPublishExcludeTypes.Contains(svcType))
                    {
                        var discoveredService = new DiscoveredService()
                                                    {
                                                        ImplementationType = type,
                                                        InterfaceType = svcType

                                                    };
                        result.Add(discoveredService);
                    }
                }
                
            }


            return result;
        }


       
    }

    public partial class Configuration
    {
        internal class DiscoveredSubscription
        {
            public Type HandlerType { get; set; }
            public Type MessageType { get; set; }

            public Type InterfaceType { get; set; }
        }
        internal class DiscoveredService
        {
            public Type InterfaceType { get; set; }
            public Type ImplementationType { get; set; }
        }

        #region Nested type: RealConfigure

        internal class RealConfigure : IBizSettings, IBusSettings, ITransportSettings, IDalSettings
        {
            #region IComponentSettings

            public Guid ComponentId { get; set; }

            public int CacheExpirationSeconds { get; set; }

            public NetworkingMode NetworkingMode { get; set; }

            public FriendComponentData FriendComponent { get; set; }

            public Type ConfigurationManagerType { get; set; }

            public bool DevLoggingActive { get; set; }

            #endregion

            #region IDalSettings

            private readonly List<DataSchemaType> schemaTypes = new List<DataSchemaType>
                                                                    {
                                                                        DataSchemaType.ClientComponent
                                                                    };

            public IList<DataSchemaType> SchemasApplied
            {
                get { return schemaTypes; }
            }

            public string ConfigurationConnectionString { get; set; }

            public DbEngineType ConfigurationSourceType { get; set; }

            #endregion

            #region IBusSettings


            public IEsbManager BusManager { get; set; }

            public IMessagePublisherDispatcherStrategy MessageDispatcher { get; set; }
            public TimeSpan SendExpiringTime { get; set; }
           

            public int MaxDelayDueToLatencySeconds { get; set; }

            #endregion

            #region ITransportSettings

            public int MaxMessageKbBeforeChunking { get; set; }

            public ushort TcpPort { get; set; }

            #endregion

            #region Internal

            public Assembly[] SearchMessageSuscriptionAssemblies { get; set; }
            public Type[] SearchMessageSuscriptionExcludeTypes { get; set; }

            public Assembly[] SearchServicesToPublishAssemblies { get; set; }

            public Type[] SearchServicesToPublishExcludeTypes { get; set; }


            #endregion
        }



        #endregion

        
    }
}