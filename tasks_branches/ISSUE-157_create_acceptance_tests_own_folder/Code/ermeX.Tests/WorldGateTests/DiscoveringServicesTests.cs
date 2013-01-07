// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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
    [Category(TestCategories.CoreFunctionalTest)]
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

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Register_Services_In_Assembly(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                         new List<DataSchemaType>(){DataSchemaType.ClientComponent})
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

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_Late_Register_Services_In_Other_Assemblies( DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                         new List<DataSchemaType>(){DataSchemaType.ClientComponent})
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