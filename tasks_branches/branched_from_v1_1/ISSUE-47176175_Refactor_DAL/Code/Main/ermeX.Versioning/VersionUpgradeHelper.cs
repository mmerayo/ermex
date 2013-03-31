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
using Ninject;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Domain.QueryDatabase;
using ermeX.Versioning.Schema.Scripts;

namespace ermeX.Versioning
{
    internal sealed class VersionUpgradeHelper : IVersionUpgradeHelper
    {
	    private readonly IQueryHelperFactory _queryHelperFactory;

	    [Inject]
		public VersionUpgradeHelper(IQueryHelperFactory queryHelperFactory)
		{
			_queryHelperFactory = queryHelperFactory;
		}

	    #region IVersionUpgradeHelper Members

        public void RunDataSchemaUpgrades(IList<DataSchemaType> schemasApplied, string configurationConnectionString,
                                          DbEngineType configurationSourceType)
        {
			//TODO: INJECT THIS RUNNER
            SchemaScriptRunner schemaScriptRunner = SchemaScriptRunner.GetRunner(schemasApplied,
                                                                                 configurationConnectionString,
                                                                                 configurationSourceType,_queryHelperFactory.GetHelper(configurationSourceType,configurationConnectionString));
            schemaScriptRunner.RunUpgrades();
        }

        #endregion
    }
}