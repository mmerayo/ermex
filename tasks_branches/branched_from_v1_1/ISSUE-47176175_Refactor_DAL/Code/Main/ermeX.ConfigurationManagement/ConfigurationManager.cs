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
using System.Configuration;
using System.Linq;
using Ninject.Modules;
using ermeX.Common;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.IoC.InjectionModules;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using Cfg = ermeX.Configuration.Configurer;


namespace ermeX.ConfigurationManagement
{
    internal class ConfigurationManager : IConfigurationManager
    {
        private static IConfigurationManager _instance;

        private IComponentSettings _settings;

        private ConfigurationManager(IComponentSettings settings)
        {
            _settings = settings;
        }

        public static IConfigurationManager Instance
        {
            get { return _instance; }
        }

        #region IConfigurationManager Members

        public void ClearConfiguration()
        {
            //TODO:
        }

        #endregion

        /// <summary>
        ///   It performs and updates the whole Service configuration
        /// </summary>
        /// <param name="settings"> </param>
        public static void SetSettingsSource(IComponentSettings settings)
        {
            //TODO  locker here
            if (settings == null) throw new ArgumentNullException("settings");
            List<string> errors;
            if (!new ComponentSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }

            if (settings.ConfigurationManagerType == null)
                _instance = new ConfigurationManager(settings);
            else
            {
                _instance = ObjectBuilder.FromType<IConfigurationManager>(settings.ConfigurationManagerType);
            }


            
            INinjectModule[] injectionModules = GetInjectionModules(settings);
            IoCManager.SetCurrentInjections(injectionModules);


            //Already disposed from previous execution and reinjected CacheProvider.ClearCache();
        }

        private static INinjectModule[] GetInjectionModules(IComponentSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

        	EnsureIocAssembliesAreInDomain();
			Type[] moduleTypes =
                TypesHelper.GetTypesFromDomainImplementing<INinjectModule>().Where(
                    x => !x.Namespace.StartsWith("Ninject")).ToArray();

            var result = new List<INinjectModule>(moduleTypes.Length);

            result.AddRange(
                moduleTypes.Select(moduleType => ObjectBuilder.FromType<INinjectModule>(moduleType, settings)));

            return result.ToArray();
        }

    	private static void EnsureIocAssembliesAreInDomain()
    	{
    		TypesHelper.GetAssemblyFromDomain("ermeX.Dal.IoC");//TODO: ADD ALL or define the injections using another mechanism
    	}


    	public static Cfg GetSettingsFromConfig()
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var target = (ermeXConfiguration) config.GetSection("ermeXConfiguration");

            //local component
            LocalComponent componentDefinition = target.ComponentDefinition;
            var result =
                Cfg.Configure(componentDefinition.ComponentId).ListeningToTcpPort((ushort) componentDefinition.TcpPort).
                    SendMessagesExpirationTime(TimeSpan.FromDays(componentDefinition.MessagesExpirationDays));
            if (componentDefinition.DiscoverSubscriptors)
                result = result.DiscoverSubscriptors();
            if (componentDefinition.DiscoverServices)
                result = result.DiscoverServicesToPublish();

            //friend component
            FriendComponentElement friendComponent = componentDefinition.FriendComponent;
            if (!friendComponent.RemoteComponentId.IsEmpty())
            {
                result = result.RequestJoinTo(friendComponent.RemoteIp, friendComponent.RemoteTcpPort,
                                              friendComponent.RemoteComponentId);
            }

            //database
            Database database = componentDefinition.Database;
            switch (database.DbType)
            {
                case DbType.InMemory:
                    result = result.SetInMemoryDb();
                    break;
                case DbType.SQLite:
                    result = result.SetSqliteDb(((PhisicalDatabase) database).ConnectionString);
                    break;
                case DbType.SqlServer:
                    result = result.SetSqlServerDb(((PhisicalDatabase) database).ConnectionString);
                    break;
            }

            return result;
        }
    }
}