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
using ermeX.Tests.Common.DataAccess;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //[TestFixture]
    internal class ConnectivityDetailsDataSourceTester :
        UpdatableByExternalComponentsTester<ConnectivityDetailsDataSource, ConnectivityDetails>
    {
        protected override string IdFieldName
        {
            get
            {
                return ConnectivityDetails.GetDbFieldName("Id");
            }
        }

        protected override string TableName
        {
            get { return string.Format("[{0}]", ConnectivityDetails.TableName); }
        }

        

        private readonly Guid serverId = Guid.NewGuid();

        protected override int InsertRecord(DbEngineType engine)
        {
            Guid cid;
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            dataAccessTestHelper.InsertAppComponent(ComponentId, ComponentOwnerId, 0, false, false);
            return dataAccessTestHelper.InsertConnectivityDetailsRecord(ComponentId, ComponentOwnerId, IP, Port, false, serverId,
                                                   DateTime.UtcNow);
        }

        protected override void CheckInsertedRecord(ConnectivityDetails record)
        {
            Assert.IsNotNull(record);
            Assert.AreEqual(record.Ip, IP);
            Assert.AreEqual(record.Port, Port);
        }

        protected override ConnectivityDetails GetExpected(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            AppComponent appComponent = dataAccessTestHelper.GetNewAppComponent();

            return new ConnectivityDetails
                       {
                           ComponentOwner = appComponent.ComponentOwner,
                           Ip = IP,
                           Port = Port,
                           ServerId = appComponent.ComponentId
                       };
        }


        protected override ConnectivityDetails GetExpectedWithChanges(ConnectivityDetails source)
        {
            source.Ip = IP2;
            return source;
        }


        protected override ConnectivityDetailsDataSource GetDataSourceTarget(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            IDalSettings dataAccessSettings = dataAccessTestHelper.DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new ConnectivityDetailsDataSource(dataAccessSettings, ComponentOwnerId,dataAccessExecutor);
        }


        private readonly Guid ComponentId = Guid.NewGuid();
        private  Guid ComponentOwnerId {get { return LocalComponentId; }}
        private const string IP = "111.222.333.123";
        private const string IP2 = "222.222.333.123";
        private const int Port = 6666;
    }
}