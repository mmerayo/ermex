// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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
    internal class ServiceDetailsSourceTester :
        UpdatableByExternalComponentsTester<ServiceDetailsDataSource, ServiceDetails>
    {
        protected override string IdFieldName
        {
            get { return ServiceDetails.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get { return ServiceDetails.TableName; }
        }

        protected override ServiceDetails GetExpectedWithChanges(ServiceDetails source)
        {
            source.ServiceImplementationTypeName = "GetExpectedWithChangesForTestType";
            return source;
        }


        protected override int InsertRecord(DbEngineType engine)
        {

            return GetDataHelper(engine).InsertServiceDetails(LocalComponentId, RemoteComponentId, serviceTestMethodName,
                                             serviceTestTypeName, serviceTestInterfaceName, operationId, DateTime.UtcNow,false);
        }

        protected override void CheckInsertedRecord(ServiceDetails record)
        {
            Assert.IsNotNull(record);
            Assert.AreEqual(operationId, record.OperationIdentifier);
            Assert.AreEqual(serviceTestTypeName, record.ServiceImplementationTypeName);
            Assert.AreEqual(serviceTestMethodName, record.ServiceImplementationMethodName);
            Assert.AreEqual(RemoteComponentId, record.Publisher);
        }

        protected override ServiceDetails GetExpected(DbEngineType engine)
        {
            return new ServiceDetails
                       {
                           ComponentOwner = OwnerComponentId,
                           OperationIdentifier = operationId,
                           ServiceImplementationTypeName = serviceTestTypeName,
                           ServiceImplementationMethodName = serviceTestMethodName,
                           ServiceInterfaceTypeName = serviceTestInterfaceName,
                           Publisher = RemoteComponentId
                       };
        }


        protected override ServiceDetailsDataSource GetDataSourceTarget(DbEngineType engine)
        {
            IDalSettings dataAccessSettings = GetDataHelper(engine).DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new ServiceDetailsDataSource(dataAccessSettings, OwnerComponentId,dataAccessExecutor);
        }

    


        private Guid OwnerComponentId { get { return LocalComponentId; } }
        private readonly Guid operationId = Guid.NewGuid();
        private string serviceTestTypeName = "TestTypeName";
        private string serviceTestMethodName = "TestMethodName";
        private string serviceTestInterfaceName = "TestInterfaceName";


        [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
        public void CanGetByOperationId(DbEngineType engine)
        {
            int id = InsertRecord(engine);

            ServiceDetailsDataSource target = GetDataSourceTarget(engine);
            ServiceDetails actual = target.GetByOperationId(RemoteComponentId, operationId);
            Assert.IsNotNull(actual);

            CheckInsertedRecord(actual);
        }

        [Ignore("Decide if this test is still useful as it needs to be modified")]
        [Test, TestCaseSource(typeof(TestCaseSources), "AllDbs")]
        public void CantInsertRepeatedOperations(DbEngineType engine)
        {
            ServiceDetails serviceDetails1 = GetExpected(engine);
            

            ServiceDetailsDataSource target = GetDataSourceTarget(engine);
            target.Save(serviceDetails1);

            ServiceDetails serviceDetails2 = GetExpected(engine);
            serviceDetails2.OperationIdentifier = Guid.NewGuid();
            serviceDetails2.Version = DateTime.UtcNow.Ticks;
            Assert.Throws<InvalidOperationException>(() => target.Save(serviceDetails2));


        }
    }
}