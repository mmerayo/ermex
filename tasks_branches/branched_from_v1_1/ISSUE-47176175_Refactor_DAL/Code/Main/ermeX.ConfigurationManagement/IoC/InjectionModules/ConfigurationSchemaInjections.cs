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
using Ninject.Modules;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data;

namespace ermeX.ConfigurationManagement.IoC.InjectionModules
{
    internal class ConfigurationSchemaInjections : NinjectModule
    {
        private readonly IDalSettings _settings;

        public ConfigurationSchemaInjections(IDalSettings settings) //TODO: TO DAL IOC
        {
            if (settings == null) throw new ArgumentNullException("settings");
            List<string> errors;
            if (!new DataAccessSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            //TODO: CHANGE THIS when configuration has values passing the whole section
            Bind<IDalSettings>().ToConstant(_settings);
        }
    }
}