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
using ServiceDetails = ermeX.DAL.Models.ServiceDetails;

namespace ermeX.DAL.DataAccess.Mappings
{
    internal abstract class ServiceDetailsMap : ClassMap<ServiceDetails>
    {
        protected abstract DbEngineType EngineType { get; }

        protected ServiceDetailsMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, ServiceDetails.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", ServiceDetails.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //
			Id(x => x.Id).GeneratedBy.Identity().Column(ServiceDetails.GetDbFieldName("Id")).Not.Nullable();
			Map(x => x.ComponentOwner).Column(ServiceDetails.GetDbFieldName("ComponentOwner")).Not.Nullable();
			Map(x => x.OperationIdentifier).Column(ServiceDetails.GetDbFieldName("OperationIdentifier")).Not.Nullable();
            Map(x => x.ServiceImplementationMethodName).Column(
				ServiceDetails.GetDbFieldName("ServiceImplementationMethodName")).Not.Nullable();
            Map(x => x.ServiceImplementationTypeName).Column(
				ServiceDetails.GetDbFieldName("ServiceImplementationTypeName")).Not.Nullable();
			Map(x => x.Publisher).Column(ServiceDetails.GetDbFieldName("Publisher")).Not.Nullable();
			Map(x => x.Version).Column(ServiceDetails.GetDbFieldName("Version")).Not.Nullable();
			Map(x => x.ServiceInterfaceTypeName).Column(ServiceDetails.GetDbFieldName("ServiceInterfaceTypeName")).Not.Nullable();
			Map(x => x.IsSystemService).Column(ServiceDetails.GetDbFieldName("IsSystemService")).Not.Nullable();
        }
    }
}