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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Mock;


namespace ermeX.Tests.Services.Receiving.Handlers.InternalMessagesHandling
{
    [TestFixture]
    internal sealed class InternalMessagesHandlerTester : DataAccessTestBase
    {
        

        private TransportMessage GetTransportMessage<TData>(ITestSettings settings, TData data)
        {
            var bizMessage = new BizMessage(data);
            var busMessage = new BusMessage(settings.ComponentId, bizMessage);
            var transportMessage = new TransportMessage(settings.ComponentId, busMessage);
            return transportMessage;
        }

        //TODO: IT would be better having a provider for the dataSources
        private BusMessageDataSource GetBusMessageDataSourceTarget(DbEngineType engine)
        {
            IDalSettings dataAccessSettings = GetDataHelper(engine).DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new BusMessageDataSource(dataAccessSettings, LocalComponentId, dataAccessExecutor);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanHandleMessage(DbEngineType engineType)
        {
            var processorWorker = new MockMessagesProcessorWorker();
            var dispatcherWorker = new MockMessagesDispatcherWorker();
            var settings = TestSettingsProvider.GetClientConfigurationSettingsSource();
            
            Assert.IsFalse(processorWorker.NewItemArrivedFlagged);
            IBusMessageDataSource busManager = GetBusMessageDataSourceTarget(engineType);
            using (var target = new InternalMessageHandler(processorWorker, dispatcherWorker, busManager,  settings))
            {
                target.StartWorkers();

                var msg= GetTransportMessage(settings, new DummyDomainEntity());
                
                target.Handle(msg);
            }

            IList<BusMessageData> busMessageDatas = busManager.GetAll();
            Assert.IsTrue(busMessageDatas.Count==1);

            //check file is in folder

            //check new item arrived was set
            Assert.IsTrue(processorWorker.NewItemArrivedFlagged);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanStartWorkers(DbEngineType engineType)
        {
            var processorWorker = new MockMessagesProcessorWorker();
            var dispatcherWorker = new MockMessagesDispatcherWorker();

            Assert.IsFalse(processorWorker.Started);
            Assert.IsFalse(dispatcherWorker.Started);

            using (
                var target = new InternalMessageHandler(processorWorker, dispatcherWorker,GetBusMessageDataSourceTarget(engineType),
                                                        TestSettingsProvider.GetClientConfigurationSettingsSource()))
                target.StartWorkers();

            Assert.IsTrue(processorWorker.Started);
            Assert.IsTrue(dispatcherWorker.Started);
        }
    }
}