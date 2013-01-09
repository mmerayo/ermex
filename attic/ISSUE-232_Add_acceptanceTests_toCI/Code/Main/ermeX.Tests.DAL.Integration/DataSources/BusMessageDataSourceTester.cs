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
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //TODO: test al possible operations in all datasources

    //[TestFixture]
    internal class BusMessageDataSourceTester :
        DataSourceTesterBase<BusMessageDataSource, BusMessageData>
    {
        private readonly DateTime VersionUtc = DateTime.UtcNow;
        private readonly Guid MessageId = Guid.NewGuid();
        private readonly Guid Publisher = Guid.NewGuid();

        private readonly DateTime CreatedTimeUtc = DateTime.Now.AddYears(5);
        private const string JsonMessage = "{\"title\": \"example\"}";
        private const BusMessageData.BusMessageStatus ExpectedStatus = BusMessageData.BusMessageStatus.SenderDispatchPending;

        protected override string IdFieldName
        {
            get
            {
                return BusMessageData.GetDbFieldName("Id");
            }
        }

        protected override string TableName
        {
            get { return BusMessageData.TableName; }
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            return GetDataHelper(engine).InsertBusMessage(OwnerComponentId, OwnerComponentId, VersionUtc, MessageId,Publisher,CreatedTimeUtc,JsonMessage, (int)ExpectedStatus);
        }

        protected override void CheckInsertedRecord(BusMessageData record)
        {
            Assert.IsNotNull(record);
            Assert.AreEqual(record.CreatedTimeUtc, CreatedTimeUtc);
            Assert.AreEqual(record.JsonMessage, JsonMessage);
            Assert.AreEqual(record.MessageId, MessageId);
            Assert.AreEqual(record.Publisher, Publisher);
        }


        protected override BusMessageData GetExpected(DbEngineType engine)
        {
            return new BusMessageData(MessageId,CreatedTimeUtc,Publisher,JsonMessage,ExpectedStatus)
                       {
                           ComponentOwner = OwnerComponentId,
                           //Version=VersionUtc.Ticks
                       };
        }


        protected override BusMessageData GetExpectedWithChanges(BusMessageData source)
        {
            source.MessageId = Guid.NewGuid();
            return source;
        }


        protected override BusMessageDataSource GetDataSourceTarget(DbEngineType engine)
        {
            IDalSettings dataAccessSettings =GetDataHelper(engine).DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new BusMessageDataSource(dataAccessSettings, OwnerComponentId,dataAccessExecutor);
        }


      

    
        private Guid OwnerComponentId {
            get { return LocalComponentId; }
        }
         
      
  
    }
}