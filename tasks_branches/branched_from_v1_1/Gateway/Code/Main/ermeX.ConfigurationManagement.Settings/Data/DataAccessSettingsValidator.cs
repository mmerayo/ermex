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
using System.Collections.Generic;

namespace ermeX.ConfigurationManagement.Settings.Data
{
    internal class DataAccessSettingsValidator : ISettingsValidator
    {
        private readonly IDalSettings _settings;

        public DataAccessSettingsValidator(IDalSettings settings)
        {
            _settings = settings;
        }

        #region ISettingsValidator Members

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            bool result = true;
            if (_settings.SchemasApplied.Count == 0)
            {
                errors.Add("Configuration:SchemasApplied: Need to define one schema");
                result = false;
            }

            if (string.IsNullOrEmpty(_settings.ConfigurationConnectionString))
            {
                errors.Add("Configuration:ConfigurationConnectionString: Need to define one connection string");
                result = false;
            }

            return result;
        }

        #endregion
    }
}