// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ermeX.Bus.Interfaces.Attributes;
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

    [Category(TestCategories.CoreFunctionalTest)]
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

            Configuration cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine,
                                                                                SchemasToApply);
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
