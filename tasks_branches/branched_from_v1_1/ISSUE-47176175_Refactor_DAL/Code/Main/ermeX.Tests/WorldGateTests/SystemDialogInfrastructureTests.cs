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
using System.Text;
using NUnit.Framework;
using ermeX;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Entities.Entities;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.WorldGateTests.Mock;

namespace ermeX.Tests.WorldGateTests
{

    [Category(TestCategories.CoreSystemTest)]
   // [TestFixture]
    class SystemDialogInfrastructureTests :DataAccessTestBase
    {
        #region Setup/Teardown

        public override void OnTearDown()
        {
            WorldGate.Reset();
            TestService.Reset();

            base.OnTearDown();
        }

        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }
        #endregion
        
        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void RegistersSystemSuscriptionsStartUp(DbEngineType dbEngine)
        { 

            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            var incommingSuscriptions = dataAccessTestHelper.GetObjectsFromDb<IncomingMessageSuscription>(IncomingMessageSuscription.TableName);

            Assert.IsNotNull(incommingSuscriptions);
            CollectionAssert.IsNotEmpty(incommingSuscriptions);

            Assert.IsTrue(incommingSuscriptions.Count(x=>x.BizMessageFullTypeName==typeof(UpdatePublishedServiceMessage).FullName && x.ComponentOwner==LocalComponentId )==1);
            Assert.IsTrue(incommingSuscriptions.Count(x => x.BizMessageFullTypeName == typeof(UpdateSuscriptionMessage).FullName && x.ComponentOwner==LocalComponentId) == 1);

            var publishedServices = dataAccessTestHelper.GetObjectsFromDb<ServiceDetails>(ServiceDetails.TableName);

            Assert.IsNotNull(publishedServices);
            CollectionAssert.IsNotEmpty(publishedServices);
            
            //Asserts that is equal than the number of methods in the interface
            Assert.IsTrue(publishedServices.Count(x => x.ServiceInterfaceTypeName == typeof(IHandshakeService).FullName && x.ComponentOwner==LocalComponentId && x.Publisher==LocalComponentId)==ServiceOperationAttribute.GetOperations(typeof(IHandshakeService)).Count());
            Assert.IsTrue(publishedServices.Count(x => x.ServiceInterfaceTypeName == typeof(IMessageSuscriptionsService).FullName && x.ComponentOwner == LocalComponentId && x.Publisher == LocalComponentId) == ServiceOperationAttribute.GetOperations(typeof(IMessageSuscriptionsService)).Count());            
            Assert.IsTrue(publishedServices.Count(x => x.ServiceInterfaceTypeName == typeof(IPublishedServicesDefinitionsService).FullName && x.ComponentOwner == LocalComponentId && x.Publisher == LocalComponentId) ==ServiceOperationAttribute.GetOperations(typeof(IPublishedServicesDefinitionsService)).Count());

        }
    }
}
