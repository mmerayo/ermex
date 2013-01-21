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
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    internal class ChunkedServiceRequestMessageDataSource : DataSource<ChunkedServiceRequestMessageData>, IChunkedServiceRequestMessageDataSource, IDataAccessUsable<ChunkedServiceRequestMessageData>
    {
        [Inject]
        public ChunkedServiceRequestMessageDataSource(IDalSettings settings, IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentSettings.ComponentId, dataAccessExecutor)
        {
        }
        public ChunkedServiceRequestMessageDataSource(IDalSettings settings, Guid componentOwner, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentOwner, dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return ChunkedServiceRequestMessageData.TableName; }
        }

        public ChunkedServiceRequestMessageData GetByCorrelationIdAndOrder(Guid correlationId, int order)
        {
            return GetItemByFields(new[]
                {new Tuple<string, object>("CorrelationId", correlationId), new Tuple<string, object>("Order", order)});
        }
    }
}