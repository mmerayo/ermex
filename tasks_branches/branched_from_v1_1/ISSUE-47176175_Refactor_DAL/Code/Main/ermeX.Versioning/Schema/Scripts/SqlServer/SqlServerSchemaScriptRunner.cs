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
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Domain.QueryDatabase;

namespace ermeX.Versioning.Schema.Scripts.SqlServer
{
    internal sealed class SqlServerSchemaScriptRunner : SchemaScriptRunner
    {
        private const string RexPrefx = "ermeX.Versioning.Schema.Scripts.SqlServer";

        public SqlServerSchemaScriptRunner(IList<DataSchemaType> schemasApplied, string configurationConnectionString, IQueryHelper queryHelper)
            : base(DbEngineType.SqlServer2008, schemasApplied, configurationConnectionString, queryHelper)
        {
        }


        protected override string NamespaceResourcesPrefix
        {
            get { return RexPrefx; }
        }

        protected override string  GetLatestVersionExecutedSqlQuery(DataSchemaType dataSchemaType)
        {
            return string.Format(
                "Select Top 1 Version_TimeStamp from [dbo].Version where Version_SchemaType={0} Order By Version_TimeStamp Desc",
                (int) dataSchemaType);
        }
    }
   
}