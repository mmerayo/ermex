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
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Entities.Entities;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.SupportTypes.Messages;
using ermeX.Tests.SupportTypes.Services;

using ermeX.Tests.WorldGateTests.Mock;

namespace ermeX.Tests.WorldGateTests
{
    [Category(TestCategories.CoreSystemTest)]
    //[TestFixture]
    internal class DiscoveringServicesTests :DataAccessTestBase
    {
        
        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }

        public override void OnTearDown()
        {
            WorldGate.Reset();
            base.OnTearDown();
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void Can_Register_Services_In_Assembly(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine)
                .DiscoverServicesToPublish(new[] { typeof(MessageA).Assembly }, new[] { typeof(IServiceB) });

            WorldGate.ConfigureAndStart(cfg);

            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            var actual = dataAccessTestHelper.GetObjectsFromDb<ServiceDetails>(ServiceDetails.TableName)
                .Where(x => !x.ServiceInterfaceTypeName.StartsWith("ermeX.Bus")).ToList();//TODO: ADD FLAG the system

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Count == typeof(IServiceA).GetMethods().Count()); 
            Assert.AreEqual(actual[0].ServiceInterfaceTypeName, typeof(IServiceA).FullName);
            Assert.AreEqual(actual[0].ServiceImplementationTypeName, typeof(ServiceA).FullName);
        }
        [Ignore("TODO")]
        [Test]
        public void Can_Register_Services_In_Assemblies([Values(DbEngineType.SqlServer2008)] DbEngineType dbEngine)
        {
            throw new NotImplementedException();
        }
        [Ignore("TODO")]
        [Test]
        public void Doesnt_Register_Services_In_Other_Assemblies([Values(DbEngineType.SqlServer2008)] DbEngineType dbEngine)
        {
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(TestCaseSources), TestCaseSources.OptionDbInMemory)]
        public void Can_Late_Register_Services_In_Other_Assemblies( DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine)
                .DiscoverServicesToPublish(new[] {typeof (MessageA).Assembly}, new[] {typeof (IServiceB)});

            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof (TestService));
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            var actual = dataAccessTestHelper.GetObjectsFromDb<ServiceDetails>(ServiceDetails.TableName)
                .Where(x => !x.ServiceInterfaceTypeName.StartsWith("ermeX.Bus")).ToList(); //TODO: ADD FLAG the system

            Assert.IsNotNull(actual);
            int count = typeof (ITestService).GetMethods().Count();
            Assert.IsTrue(actual.Count == typeof(IServiceA).GetMethods().Count() + count);
            Assert.AreEqual(actual[0].ServiceInterfaceTypeName, typeof (IServiceA).FullName);
            Assert.AreEqual(actual[0].ServiceImplementationTypeName, typeof (ServiceA).FullName);
        }

       

    }
}