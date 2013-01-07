// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Common;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Entities.Entities;
using ermeX.Interfaces;
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
            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
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


            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                   SchemasToApply);
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