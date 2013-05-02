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
using ermeX.DAL.Mappings.UserMappingTypes;
using ermeX.Models.Entities;

namespace ermeX.DAL.Mappings
{
    internal abstract class IncomingMessagesMap : ClassMap<IncomingMessage>
    {
        protected abstract DbEngineType EngineType { get; }

        protected IncomingMessagesMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, IncomingMessage.FinalTableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", IncomingMessage.FinalTableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);

			Id(x => x.Id).GeneratedBy.Identity().Column(IncomingMessage.GetDbFieldName("Id")).Not.Nullable();
			Map(x => x.PublishedBy).Column(IncomingMessage.GetDbFieldName("PublishedBy")).Not.Nullable();
			Map(x => x.PublishedTo).Column(IncomingMessage.GetDbFieldName("PublishedTo")).Not.Nullable();
			Map(x => x.SuscriptionHandlerId).Column(IncomingMessage.GetDbFieldName("SuscriptionHandlerId")).Not.Nullable();           
            Map(x => x.TimeReceivedUtc).Column(IncomingMessage.GetDbFieldName("TimeReceivedUtc")).CustomType(
				typeof(DateTimeUserType)).Not.Nullable();
			Map(x => x.ComponentOwner).Column(IncomingMessage.GetDbFieldName("ComponentOwner")).Not.Nullable();
			Map(x => x.Version).Column(IncomingMessage.GetDbFieldName("Version")).Not.Nullable();

			Map(x => x.Status).Column(IncomingMessage.GetDbFieldName("Status")).CustomType<Message.MessageStatus>().Not.Nullable();
			Map(x => x.JsonMessage).Column(IncomingMessage.GetDbFieldName("JsonMessage")).Not.Nullable();
			Map(x => x.MessageId).Column(IncomingMessage.GetDbFieldName("MessageId")).Not.Nullable();
            Map(x => x.CreatedTimeUtc).Column(IncomingMessage.GetDbFieldName("CreatedTimeUtc")).CustomType(
				typeof(DateTimeUserType)).Not.Nullable(); ;


        }
    }
}