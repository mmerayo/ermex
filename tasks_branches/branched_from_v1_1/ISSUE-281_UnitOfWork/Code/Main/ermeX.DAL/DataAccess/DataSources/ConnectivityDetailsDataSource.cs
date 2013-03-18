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
using NHibernate;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    //TODO:refactor to base
    internal class ConnectivityDetailsDataSource : DataSource<ConnectivityDetails>, IConnectivityDetailsDataSource, IDataAccessUsableConnectivityDetails
    {
        [Inject]
        public ConnectivityDetailsDataSource(IDalSettings dataAccessSettings,
                                             IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : this(dataAccessSettings, componentSettings.ComponentId,dataAccessExecutor)
        {
        }

        public ConnectivityDetailsDataSource(IDalSettings dataAccessSettings, Guid componentId,IDataAccessExecutor dataAccessExecutor)
            : base(dataAccessSettings, componentId,dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return ConnectivityDetails.TableName; }
        }

        #region IConnectivityDetailsDataSource Members

        public ConnectivityDetails GetByComponentId(Guid componentId)
        {
            //The priority is the order
            return GetItemByField("ServerId", componentId); //TODO: OPTIMIZE THIS. 
        }

        public ConnectivityDetails GetByComponentId(ISession session,Guid componentId)
        {
            //The priority is the order
            return GetItemByField(session,"ServerId", componentId); //TODO: OPTIMIZE THIS. 
        }

        #endregion

        protected override bool BeforeUpdating(ConnectivityDetails entity, ISession session)
        {
            if (entity.ServerId.IsEmpty())
                throw new ArgumentException("Entity has not a valid property value, serverId");

            var existing = GetByComponentId(session,entity.ServerId);
            session.Evict(existing);
            if (existing == null)
                return true;

            if (existing.Version > entity.Version)
                return false;
            return true;
        }

       
        protected override void UpdateWhenExternal(ConnectivityDetails entity, ConnectivityDetails existingEntity)
        {
            base.UpdateWhenExternal(entity, existingEntity);
            entity.IsLocal = existingEntity.IsLocal;
        }
    }
}