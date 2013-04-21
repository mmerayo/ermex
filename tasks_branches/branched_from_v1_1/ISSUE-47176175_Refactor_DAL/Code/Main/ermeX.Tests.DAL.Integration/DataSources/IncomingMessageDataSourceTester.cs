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
using ermeX.DAL.Repository;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //[TestFixture]
    internal class IncomingMessageDataSourceTester : DataSourceTesterBase<Repository<IncomingMessage>, IncomingMessage>
    {
        
        private readonly DateTime TimePublished = new DateTime(2010, 2, 3, 1, 2, 3, 330);
        private readonly DateTime TimeReceived = new DateTime(2010, 2, 3, 3, 4, 5, 330);
       
        private readonly Guid BMID = Guid.NewGuid();

        private readonly Guid componentId = Guid.NewGuid();
        private Guid ownerCompId {get { return LocalComponentId; }}
        private readonly DummyDomainEntity message = new DummyDomainEntity {Id = Guid.NewGuid()};
        private readonly Guid suscriptionHandlerId = Guid.NewGuid();


        protected override string IdFieldName
        {
            get
            {
                return IncomingMessage.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get { return IncomingMessage.FinalTableName; }
        }
       

        protected override IncomingMessage GetExpectedWithChanges(IncomingMessage source)
        {
            //source.BusMessageId = BMID2;
            return source;
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            dataAccessTestHelper.InsertAppComponent(componentId, ownerCompId,0,false,false);

            return dataAccessTestHelper.InsertIncomingMessage(componentId, ownerCompId, BMID, 
                                         TimePublished, TimeReceived, suscriptionHandlerId,Message.MessageStatus.ReceiverReceived, new BizMessage("TheMessage").JsonMessage);
        }

        protected override void CheckInsertedRecord(IncomingMessage record)
        {
            Assert.IsNotNull(record);
            Assert.IsTrue(record.PublishedTo == componentId);
            Assert.IsTrue(record.PublishedBy == ownerCompId);
            //Assert.IsTrue(record.BusMessageId == BMID);
#if (!NEED_FIX_MILLISECONDS)
            Assert.IsTrue(record.CreatedTimeUtc == TimePublished);
            Assert.IsTrue(record.TimeReceivedUtc == TimeReceived);

#endif
            Assert.IsTrue(record.ComponentOwner == ownerCompId);
            Assert.IsTrue(record.SuscriptionHandlerId == suscriptionHandlerId);
        }

        protected override IncomingMessage GetExpected(DbEngineType engine)
        {
            BusMessage busMessage = GetBusMessage(message);
            return new IncomingMessage(busMessage)
                       {
                           ComponentOwner = ownerCompId,
                           PublishedBy = ownerCompId,
                           PublishedTo = componentId,
                           CreatedTimeUtc = TimePublished,
                           TimeReceivedUtc = TimeReceived,
                           SuscriptionHandlerId = suscriptionHandlerId,
                           Status=Message.MessageStatus.SenderCollected
                       };
        }

        private BusMessage GetBusMessage<TMessage>(TMessage data)
        {
            return new BusMessage(ownerCompId, new BizMessage(data));
        }

    }
}