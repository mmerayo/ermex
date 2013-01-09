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
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //TODO: test al possible operations in all datasources

    //[TestFixture]
    internal class AppComponentDataSourceTester :
        UpdatableByExternalComponentsTester<AppComponentDataSource, AppComponent>
    {
        protected override string IdFieldName
        {
            get
            {
                return AppComponent.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get { return AppComponent.TableName; }
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            return GetDataHelper(engine).InsertAppComponent(ComponentId, OwnerComponentId, VersionUtc, Latency,IsRunning,ExchangedDefinitions);
        }

        


        protected override void CheckInsertedRecord(AppComponent record)
        {
            Assert.IsNotNull(record);
            Assert.AreEqual(record.ComponentId, ComponentId);
        }

        private const int Latency = 10;

        protected override AppComponent GetExpected(DbEngineType engine)
        {
            return new AppComponent
                       {
                           ComponentId = ComponentId,
                           ComponentOwner = OwnerComponentId,
                           Latency = Latency,
                           IsRunning = IsRunning
                       };
        }


        protected override AppComponent GetExpectedWithChanges(AppComponent source)
        {
            source.IsRunning = false;
            return source;
        }


        protected override AppComponentDataSource GetDataSourceTarget(DbEngineType engine)
        {
            IDalSettings dataAccessSettings =GetDataHelper(engine).DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new AppComponentDataSource(dataAccessSettings, OwnerComponentId,dataAccessExecutor);
        }


        private Guid ComponentId
        {
            get { return RemoteComponentId; }
        }

    
        private Guid OwnerComponentId {
            get { return LocalComponentId; }
        }
         
        private readonly DateTime VersionUtc = DateTime.UtcNow;
        private bool IsRunning = true;
        private bool ExchangedDefinitions=true;
        
        
        [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
        public void CanGetByComponentId(DbEngineType engine)
        {
            int id = InsertRecord(engine);
            Assert.IsTrue(id>0);
            AppComponentDataSource target = GetDataSourceTarget(engine);
            AppComponent actual = target.GetByComponentId(ComponentId);
            Assert.IsNotNull(actual);

            CheckInsertedRecord(actual);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
        public void CanUpdateExchanger(DbEngineType engine)
        {
            int id = InsertRecord(engine);
            Assert.IsTrue(id > 0);
            AppComponentDataSource target = GetDataSourceTarget(engine);
            AppComponent actual = target.GetByComponentId(ComponentId);
            Assert.IsNotNull(actual);
            Assert.IsNull(actual.ComponentExchanges);

            actual.ComponentExchanges = RemoteComponentId;
            target.Save(actual);

            var actual2= target.GetByComponentId(ComponentId);
            Assert.IsNotNull(actual2.ComponentExchanges);

            Assert.AreEqual(actual.ComponentExchanges, actual2.ComponentExchanges);
        }
    }
}