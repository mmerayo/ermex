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
using System.Linq;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    internal class BusMessageDataSource : DataSource<BusMessageData>, IBusMessageDataSource, IDataAccessUsable<BusMessageData>
    {
        [Inject]
        public BusMessageDataSource(IDalSettings settings, IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentSettings.ComponentId, dataAccessExecutor)
        {
        }

        public BusMessageDataSource(IDalSettings settings, Guid componentOwner, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentOwner, dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return BusMessageData.TableName; }
        }
        public static string GetTableName()
        {
            return BusMessageData.TableName; 
        }

        public IList<BusMessageData> GetMessagesToDispatch()
        {
           return GetItemsByField("Status", BusMessageData.BusMessageStatus.ReceiverReceived).OrderBy(x=>x.CreatedTimeUtc).ToList();
        }
    }
}