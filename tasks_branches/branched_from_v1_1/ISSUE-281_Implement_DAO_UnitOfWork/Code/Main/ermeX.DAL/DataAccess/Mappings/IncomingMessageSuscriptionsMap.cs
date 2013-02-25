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
using ermeX.DAL.DataAccess.Mappings.UserMappingTypes;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.Mappings
{
    internal abstract class IncomingMessageSuscriptionsMap : ClassMap<IncomingMessageSuscription>
    {
        protected abstract DbEngineType EngineType { get; }

        protected IncomingMessageSuscriptionsMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, IncomingMessageSuscription.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", IncomingMessageSuscription.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(IncomingMessageSuscription.GetDbFieldName("Id"));
            Map(x => x.DateLastUpdateUtc).Column(IncomingMessageSuscription.GetDbFieldName("DateLastUpdateUtc")).
                CustomType(typeof (DateTimeUserType));
            Map(x => x.BizMessageFullTypeName).Column(IncomingMessageSuscription.GetDbFieldName("BizMessageFullTypeName"))
                .Length(256);
            Map(x => x.ComponentOwner).Column(IncomingMessageSuscription.GetDbFieldName("ComponentOwner"));
            Map(x => x.SuscriptionHandlerId).Column(IncomingMessageSuscription.GetDbFieldName("SuscriptionHandlerId"));
            Map(x => x.Version).Column(IncomingMessageSuscription.GetDbFieldName("Version"));
            Map(x => x.HandlerType).Column(IncomingMessageSuscription.GetDbFieldName("HandlerType"));
            //TODO:restrictions composite key
        }
    }
}