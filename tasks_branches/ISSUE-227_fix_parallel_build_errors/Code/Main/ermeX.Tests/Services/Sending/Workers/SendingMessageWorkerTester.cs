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
using System.Data;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Publishing.AsyncWorkers;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;

using ermeX.Entities.Entities;
using ermeX.LayerMessages;

using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Mock;


namespace ermeX.Tests.Services.Sending.Workers
{
    //[TestFixture]
    internal sealed class SendingMessageWorkerTester :DataAccessTestBase
    {

       
        private const string FolderName = "TestData";
        private const int BMID = 222;

        public override void  OnStartUp()

        {
            string testDataPath = PathUtils.GetApplicationFolderPath(FolderName);

            if (!Directory.Exists(testDataPath))
                Directory.CreateDirectory(testDataPath);
             base.OnStartUp();
        }

        public override void OnTearDown()
        {
            string testDataPath = PathUtils.GetApplicationFolderPath(FolderName);

            if (Directory.Exists(testDataPath))
            {
                Directory.Delete(testDataPath, true);
            }

           base.OnTearDown();
        }


        private SendingMessageWorker GetTarget(DbEngineType engineType, out DummyServiceProxy proxy,
                                               out OutgoingMessagesDataSource dataSource,
                                               out TestSettingsProvider.ClientSettings settings)
        {
            var dataAccessExecutor = GetdataAccessExecutor(engineType);
            dataSource =
                new OutgoingMessagesDataSource(
                    dataAccessExecutor.DalSettings,
                    LocalComponentId,dataAccessExecutor);

            BusMessageDataSource busMessageDataSource=GetBusMessageDataSource(engineType);

            settings = (TestSettingsProvider.ClientSettings) TestSettingsProvider.GetClientConfigurationSettingsSource();
            proxy = new DummyServiceProxy();
            var target = new SendingMessageWorker(dataSource, busMessageDataSource, settings, proxy);
            return target;
        }

        private readonly Dictionary<DbEngineType, DataAccessExecutor> _dataAccessExecutors = new Dictionary<DbEngineType, DataAccessExecutor>();
        private DataAccessExecutor GetdataAccessExecutor(DbEngineType engineType)
        {
            if (!_dataAccessExecutors.ContainsKey(engineType))
            {
                var dataAccessExecutor = new DataAccessExecutor(GetDataHelper(engineType).DataAccessSettings);
                _dataAccessExecutors.Add(engineType, dataAccessExecutor);
            }
            return _dataAccessExecutors[engineType];
        }
        private readonly Dictionary<DbEngineType,BusMessageDataSource> _busMessageDataSources=new Dictionary<DbEngineType, BusMessageDataSource>(); 
        private BusMessageDataSource GetBusMessageDataSource(DbEngineType engineType)
        {
            if (!_busMessageDataSources.ContainsKey(engineType))
            {
                var dataAccessExecutor = GetdataAccessExecutor(engineType);
                var busMessageDataSource = new BusMessageDataSource(dataAccessExecutor.DalSettings, LocalComponentId,
                                                                    dataAccessExecutor);
                _busMessageDataSources.Add(engineType, busMessageDataSource);
            }
            return _busMessageDataSources[engineType];
        }

        private SendingMessageWorker GetTarget(DbEngineType engineType, out DummyServiceProxy proxy,
                                               out OutgoingMessagesDataSource dataSource)
        {
            TestSettingsProvider.ClientSettings settings;
            return GetTarget(engineType, out proxy, out dataSource, out settings);
        }

        private SendingMessageWorker GetTarget(DbEngineType engineType, out DummyServiceProxy proxy)
        {
            OutgoingMessagesDataSource ds;
            return GetTarget(engineType, out proxy, out ds);
        }

        private SendingMessageWorker GetTarget(DbEngineType engineType)
        {
            DummyServiceProxy none;
            OutgoingMessagesDataSource ds;
            return GetTarget(engineType, out none, out ds);
        }

        private BusMessage GetBusMessage<TMessage>(TMessage data)
        {
            var bizMessage = new BizMessage(data);
            var busMessage = new BusMessage(LocalComponentId, bizMessage);
            return busMessage;
        }


        private OutgoingMessage GetExpected(DbEngineType engineType)
        {
            BusMessageDataSource busMessageDataSource = GetBusMessageDataSource(engineType);

            var msg = new DummyDomainEntity {Id = Guid.NewGuid()};
            BusMessage busMessage = GetBusMessage(msg);
            BusMessageData fromBusLayerMessage = BusMessageData.FromBusLayerMessage(LocalComponentId, busMessage, BusMessageData.BusMessageStatus.ReceiverDispatchable);
            fromBusLayerMessage.ComponentOwner = LocalComponentId;
            busMessageDataSource.Save(fromBusLayerMessage);
            var expected = new OutgoingMessage(fromBusLayerMessage)
                               {
                                   PublishedTo = RemoteComponentId,
                                   TimePublishedUtc = DateTime.UtcNow,
                                   ComponentOwner = LocalComponentId
                               };
            
            
            return expected;
        }

        private bool _finishedFlag;

        private void target_Finished(object sender, EventArgs eventArgs)
        {
            _finishedFlag = true;
        }

        [Ignore]
        [Test]
        public void CanBeCanceled()
        {
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanSendMessage(DbEngineType engineType)
        {
            DummyServiceProxy proxy;
            SendingMessageWorker target = GetTarget(engineType, out proxy);
            OutgoingMessage expected = GetExpected(engineType);
            BusMessageDataSource busMessageDataSource = GetBusMessageDataSource(engineType);
            BusMessageData busMessageData = busMessageDataSource.GetById(expected.BusMessageId);
            Assert.IsNotNull(busMessageData);
            target.StartWorking(expected);
            target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(20));
            target.Dispose();

            Assert.AreEqual(busMessageData.MessageId, proxy.LastSentMessage.Data.MessageId);
            Assert.IsNull(busMessageDataSource.GetById(expected.BusMessageId));


        }


        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CantSendMessageWhenNotSetUp(DbEngineType engineType)
        {
            DummyServiceProxy proxy;
            SendingMessageWorker target = GetTarget(engineType, out proxy);
            target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(10));
            target.Dispose();
            Assert.IsNull(proxy.LastSentMessage);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void DeletesMessageRecordAfterSending(DbEngineType engineType)
        {
            DummyServiceProxy proxy;
            OutgoingMessagesDataSource ds;
            SendingMessageWorker target = GetTarget(engineType, out proxy, out ds);
            OutgoingMessage message = GetExpected(engineType);
            BusMessage expected = GetExistingBusMessageData(engineType, message.BusMessageId);

            ds.Save(message);
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engineType);
            Assert.IsNotNull(dataAccessTestHelper.GetObjectFromDb<OutgoingMessage>(message.Id,
                                                                   OutgoingMessage.FinalTableName));

            target.StartWorking(message);
            target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(20));
            target.Dispose();

            BusMessage actual = proxy.LastSentMessage.Data;
            Assert.AreEqual(expected.CreatedTimeUtc, actual.CreatedTimeUtc);
            Assert.AreEqual(expected.Publisher, actual.Publisher);
            Assert.AreEqual(expected.MessageId, actual.MessageId);
            Assert.AreEqual(expected.Data.JsonMessage, actual.Data.JsonMessage);

            Assert.IsNull(dataAccessTestHelper.GetObjectFromDb<OutgoingMessage>(message.Id,
                                                                OutgoingMessage.FinalTableName));
        }

        private BusMessageData GetExistingBusMessageData(DbEngineType engineType, int id)
        {
            BusMessageDataSource busMessageDataSource = GetBusMessageDataSource(engineType);

            BusMessageData busMessageData = busMessageDataSource.GetById(id);
            return busMessageData;
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void DontDeleteMessageFileAfterSendingWhenMoreSuscribers(DbEngineType engineType)
        {
            DummyServiceProxy proxy;
            OutgoingMessagesDataSource ds;
            SendingMessageWorker target = GetTarget(engineType, out proxy, out ds);
            OutgoingMessage expected = GetExpected(engineType);
            OutgoingMessage exp2 = GetExpected(engineType);

            ds.Save(expected);
            ds.Save(exp2);

            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engineType);
            DataTable tableFromDb = dataAccessTestHelper.GetTableFromDb(OutgoingMessage.FinalTableName);
            Assert.IsNotNull(tableFromDb);
            Assert.IsTrue(tableFromDb.Rows.Count == 2);
            BusMessageData existingBusMessageData = GetExistingBusMessageData(engineType, expected.BusMessageId);

            target.StartWorking(expected);
            target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(20));
            target.Dispose();

            BusMessage actual = proxy.LastSentMessage.Data;
            Assert.AreEqual(existingBusMessageData.CreatedTimeUtc, actual.CreatedTimeUtc);
            Assert.AreEqual(existingBusMessageData.Publisher, actual.Publisher);
            Assert.AreEqual(existingBusMessageData.MessageId, actual.MessageId);
            Assert.AreEqual(existingBusMessageData.JsonMessage, actual.Data.JsonMessage);

            Assert.IsNotNull(dataAccessTestHelper.GetObjectFromDb<OutgoingMessage>(exp2.Id, OutgoingMessage.FinalTableName));
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void WhenFinishesReportsSuscriber(DbEngineType engineType)
        {
            DummyServiceProxy proxy;
            SendingMessageWorker target = GetTarget(engineType, out proxy);
            OutgoingMessage expected = GetExpected(engineType);
            _finishedFlag = false;
            target.ThreadFinished += target_Finished;
            target.StartWorking(expected);
            target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(20));
            target.Dispose();

            Assert.IsTrue(_finishedFlag);
        }


        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void WhenMessageCantBeSentIncreasesTries(DbEngineType engineType)
        {
            DummyServiceProxy proxy;
            OutgoingMessagesDataSource ds;
            TestSettingsProvider.ClientSettings settings;

            SendingMessageWorker target = GetTarget(engineType, out proxy, out ds, out settings);
            OutgoingMessage expected = GetExpected(engineType);
            ds.Save(expected);

            proxy.ForceNumTries = 20;

             target.StartWorking(expected);
            
            

            for (int i = 0; i < proxy.ForceNumTries; i++)
            {
                target.WorkPendingEvent.Set();
                target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(5));
            }

            target.Dispose();
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engineType);
            var msg = dataAccessTestHelper.GetObjectFromDb<OutgoingMessage>(expected.Id, OutgoingMessage.FinalTableName);
            Assert.IsNull(msg);

            //TODO: ASSERTION REMOVED DUE TO ISSUE-16 check correctness
            //Assert.IsTrue(proxy.ForceNumTries - 1 == ((OutgoingMessage)proxy.LastSentMessage).Tries);
            //    //-1 as is the las one captured
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void WhenMessageHasBeenExpiredIsMarkedAsFailed(DbEngineType engineType)
        {
            DummyServiceProxy proxy;
            OutgoingMessagesDataSource ds;
            TestSettingsProvider.ClientSettings settings;

            SendingMessageWorker target = GetTarget(engineType, out proxy, out ds, out settings);
            OutgoingMessage expected = GetExpected(engineType);
            ds.Save(expected);

            Assert.IsFalse(expected.Failed);
            settings.SendExpiringTime = TimeSpan.FromSeconds(1);
            proxy.ForceNumTries = 5;

            target.StartWorking(expected);
            

            for (int i = 0; i < proxy.ForceNumTries ; i++)
            {
                target.WorkPendingEvent.Set();
                target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(5));
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            target.Dispose();
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engineType);
            var msg = dataAccessTestHelper.GetObjectFromDb<OutgoingMessage>(expected.Id, OutgoingMessage.FinalTableName);
            Assert.IsNotNull(msg);
            Assert.IsTrue(msg.Failed);
            Assert.IsTrue(msg.Tries == 0 || msg.Tries == 1, "msg.Tries:" + msg.Tries);
        }
    }
}