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
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.RandomValues;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //TODO: test al possible operations in all datasources

    //[TestFixture]
    internal class ChunkedServiceRequestMessageDataSourceTester :
        DataSourceTesterBase<ChunkedServiceRequestMessageDataSource, ChunkedServiceRequestMessageData>
    {
        private readonly DateTime VersionUtc = DateTime.UtcNow;
        private readonly Guid Operation = Guid.NewGuid();
        private readonly Guid CorrelationId = Guid.NewGuid();
        private readonly byte[] Data=new byte[]{12,13,14,15};
        private readonly int Order =RandomHelper.GetRandomInt(123, 123123);
        private readonly bool Eof = true;
        private Guid OwnerComponentId
        {
            get { return LocalComponentId; }
        }
         
        protected override string IdFieldName
        {
            get
            {
                return ChunkedServiceRequestMessageData.GetDbFieldName("Id");
            }
        }

        protected override string TableName
        {
            get { return ChunkedServiceRequestMessageData.TableName; }
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            return GetDataHelper(engine).InsertChunkedServiceRequestMessageData(OwnerComponentId, OwnerComponentId, VersionUtc, CorrelationId, Data, Eof, Operation, Order);
        }

        protected override void CheckInsertedRecord(ChunkedServiceRequestMessageData record)
        {
            Assert.IsNotNull(record);
            Assert.AreEqual(record.CorrelationId, CorrelationId);
            Assert.AreEqual(record.Data, Data);
            Assert.AreEqual(record.Eof, Eof);
            Assert.AreEqual(record.Operation, Operation);
            Assert.AreEqual(record.Order, Order);
        }


        protected override ChunkedServiceRequestMessageData GetExpected(DbEngineType engine)
        {
            return new ChunkedServiceRequestMessageData(){
                           ComponentOwner = OwnerComponentId,
                           CorrelationId = CorrelationId,
                           Data=new List<byte>(Data),
                           Eof = Eof,
                           Operation=Operation,
                           Order=Order,
                       };
        }


        protected override ChunkedServiceRequestMessageData GetExpectedWithChanges(ChunkedServiceRequestMessageData source)
        {
            source.CorrelationId = Guid.NewGuid();
            return source;
        }


      
  
    }
}