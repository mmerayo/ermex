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
using System.Reflection;
using NUnit.Framework;
using ermeX;
using ermeX.Common;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Entities.Entities;

using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.SupportTypes.Handlers;
using ermeX.Tests.SupportTypes.Messages;

using ermeX.Tests.WorldGateTests.Mock;

namespace ermeX.Tests.WorldGateTests
{
    [Category(TestCategories.CoreFunctionalTest)]
    //[TestFixture]
    internal class RegisteringServicesTests : RegisteringTestsBase
    {
       

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_Service(DbEngineType dbEngine)
        {
            DoTestRegisterService(dbEngine);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Update_Service_Registration_When_Published_ByLocal_Component(DbEngineType dbEngine)
        {
            DoTestRegisterService(dbEngine);
            WorldGate.Reset();
            Assert.DoesNotThrow(() => DoTestRegisterService(dbEngine));
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Cant_Register_Biz_Service_When_Published_ByAnother_Component_and_ReturnValues(
            DbEngineType dbEngine)
        {
            DoRepeatedServiceRegistrationTest(dbEngine,false);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_Biz_Service_When_Published_ByAnother_Component_and_Dont_ReturnValues(
            DbEngineType dbEngine)
        {
            bool previousCreateDbValue = CreateDatabase;
            CreateDatabase = true;
            try
            {
                DoRepeatedServiceRegistrationTest<ITestService2>(dbEngine, false);
            }finally
            {
                CreateDatabase = previousCreateDbValue;
            }
        }



        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_System_Service_When_Published_ByAnother_Component(
            DbEngineType dbEngine)
        {
            DoRepeatedServiceRegistrationTest(dbEngine,true);
        }

        #region private methods
        

        private void DoTestRegisterService(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof(TestService));

            MethodInfo[] methods = TypesHelper.GetPublicInstanceMethods(typeof(ITestService));
            List<ServiceDetails> services = GetDataHelper(dbEngine).GetList<ServiceDetails>().Where(x => methods.Select(y => y.Name).Contains(x.ServiceImplementationMethodName)).ToList();

            Assert.IsTrue(services.Count == methods.Length);

            foreach (ServiceDetails svc in services)
            {
                Assert.IsNotNull(svc);

                Assert.IsTrue(svc.ComponentOwner == LocalComponentId);

                Assert.IsTrue(svc.ServiceInterfaceTypeName == typeof(ITestService).FullName);
                Assert.IsTrue(svc.ServiceImplementationTypeName == typeof(TestService).FullName);
            }

            foreach (MethodInfo method in methods)
            {
                Assert.IsTrue(services.Count(x => x.ServiceImplementationMethodName == method.Name) == 1);
            }
        }

      

        private void DoRepeatedServiceRegistrationTest(DbEngineType dbEngine, bool asSystemService)
        {
            //var type = typeof (ITestService);
            DoRepeatedServiceRegistrationTest<ITestService>(dbEngine, asSystemService);
        }

        private void DoRepeatedServiceRegistrationTest<TService>(DbEngineType dbEngine, bool asSystemService) where TService : IService
        {
            const string methodName = "EmptyMethod";
            Type serviceType = typeof(TService);

            MethodInfo[] publicInstanceMethods = TypesHelper.GetPublicInstanceMethods(serviceType);
            bool returnValues = publicInstanceMethods.Any(x => x.ReturnType != typeof(void));

            var operationIdentifier = ServiceOperationAttribute.GetOperationIdentifier(serviceType, methodName);
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(dbEngine);
            dataAccessTestHelper.InsertServiceDetails(LocalComponentId, Guid.NewGuid(), methodName, "TestTypeName",
                                 serviceType.FullName, operationIdentifier, DateTime.UtcNow, asSystemService);


            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);
            TestDelegate testDelegate = () => WorldGate.RegisterService<TService>(typeof(TestService));
            if (asSystemService || !returnValues)
                Assert.DoesNotThrow(testDelegate);
            else
                Assert.Throws<InvalidOperationException>(testDelegate);
        } 
        #endregion
    }
}