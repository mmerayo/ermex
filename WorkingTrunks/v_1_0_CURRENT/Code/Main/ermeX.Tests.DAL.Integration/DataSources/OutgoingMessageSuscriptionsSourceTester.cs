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
    internal class OutgoingMessageSuscriptionsSourceTester :
        UpdatableByExternalComponentsTester<OutgoingMessageSuscriptionsDataSource, OutgoingMessageSuscription>
    {
        private static readonly Guid _componentId = Guid.NewGuid(); //TODO: CHECK IF ITS REDUNDANT
        private static readonly DateTime _updateTime = new DateTime(2010, 12, 12, 12, 12, 12, 330);
        private string _messageType = "Test.Type";

        protected override string IdFieldName
        {
            get { return OutgoingMessageSuscription.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get { return OutgoingMessageSuscription.TableName; }
        }

       

        protected override OutgoingMessageSuscription GetExpectedWithChanges(OutgoingMessageSuscription source)
        {
            source.BizMessageFullTypeName = "another";
            return source;
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            Guid cid;
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            dataAccessTestHelper.InsertAppComponent(_componentId, LocalComponentId,0,false,false);

            return dataAccessTestHelper.InsertOutgoingMessageSuscriptions(_messageType, _updateTime, _componentId, LocalComponentId);
        }

        protected override void CheckInsertedRecord(OutgoingMessageSuscription record)
        {
            Assert.IsNotNull(record);
            Assert.IsTrue(record.Component == _componentId);
#if !NEED_FIX_MILLISECONDS
            Assert.IsTrue(record.DateLastUpdateUtc == _updateTime); //TODO:milliseconds are truncated
#endif
            Assert.AreEqual(_messageType, record.BizMessageFullTypeName);
        }

        protected override OutgoingMessageSuscription GetExpected(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            AppComponent appComponent = dataAccessTestHelper.GetNewAppComponent();

            return new OutgoingMessageSuscription
                       {
                           Component = appComponent.ComponentId,
                           ComponentOwner = appComponent.ComponentOwner,
                           BizMessageFullTypeName = _messageType,
                           DateLastUpdateUtc = _updateTime
                       };
        }

        protected override OutgoingMessageSuscriptionsDataSource GetDataSourceTarget(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            IDalSettings dataAccessSettings = dataAccessTestHelper.DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new OutgoingMessageSuscriptionsDataSource(dataAccessSettings, LocalComponentId,dataAccessExecutor);
        }
    }
}