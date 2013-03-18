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
using ermeX.Tests.Common.DataAccess;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //[TestFixture]
    internal class OutgoingMessageSuscriptionsSourceTester :
        UpdatableByExternalComponentsTester<OutgoingMessageSuscriptionsDataSource, OutgoingMessageSuscription>
    {
        private static readonly Guid _componentId = Guid.NewGuid(); //TODO: CHECK IF ITS REDUNDANT
        private static readonly DateTime _updateTime = new DateTime(2010, 12, 12, 12, 12, 12, 330);
        private string _messageType = "Test.Type";

        protected override string IdFieldName
        {
            get { return OutgoingMessageSuscription.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get { return OutgoingMessageSuscription.TableName; }
        }

       

        protected override OutgoingMessageSuscription GetExpectedWithChanges(OutgoingMessageSuscription source)
        {
            source.BizMessageFullTypeName = "another";
            return source;
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            Guid cid;
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            dataAccessTestHelper.InsertAppComponent(_componentId, LocalComponentId,0,false,false);

            return dataAccessTestHelper.InsertOutgoingMessageSuscriptions(_messageType, _updateTime, _componentId, LocalComponentId);
        }

        protected override void CheckInsertedRecord(OutgoingMessageSuscription record)
        {
            Assert.IsNotNull(record);
            Assert.IsTrue(record.Component == _componentId);
#if !NEED_FIX_MILLISECONDS
            Assert.IsTrue(record.DateLastUpdateUtc == _updateTime); //TODO:milliseconds are truncated
#endif
            Assert.AreEqual(_messageType, record.BizMessageFullTypeName);
        }

        protected override OutgoingMessageSuscription GetExpected(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            AppComponent appComponent = dataAccessTestHelper.GetNewAppComponent();

            return new OutgoingMessageSuscription
                       {
                           Component = appComponent.ComponentId,
                           ComponentOwner = appComponent.ComponentOwner,
                           BizMessageFullTypeName = _messageType,
                           DateLastUpdateUtc = _updateTime
                       };
        }

      
    }
}