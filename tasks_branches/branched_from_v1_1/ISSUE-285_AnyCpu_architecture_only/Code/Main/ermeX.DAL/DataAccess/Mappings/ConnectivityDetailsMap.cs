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
using FluentNHibernate.Mapping;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.Mappings
{
    internal abstract class ConnectivityDetailsMap : ClassMap<ConnectivityDetails>
    {
        protected abstract DbEngineType EngineType { get; }

        protected ConnectivityDetailsMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, ConnectivityDetails.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", ConnectivityDetails.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(ConnectivityDetails.GetDbFieldName("Id"));
            Map(x => x.Ip).Column(ConnectivityDetails.GetDbFieldName("Ip")).Unique();
            Map(x => x.Port).Column(ConnectivityDetails.GetDbFieldName("Port"));
            Map(x => x.IsLocal).Column(ConnectivityDetails.GetDbFieldName("IsLocal"));
            Map(x => x.ServerId).Column(ConnectivityDetails.GetDbFieldName("ServerId"));
            Map(x => x.ComponentOwner).Column(ConnectivityDetails.GetDbFieldName("ComponentOwner"));
            Map(x => x.Version).Column(ConnectivityDetails.GetDbFieldName("Version"));
            //TODO:restrictions composite key
        }
    }
}