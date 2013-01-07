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
    internal class IncomingMessageSuscriptionsSourceTester :
        DataSourceTesterBase<IncomingMessageSuscriptionsDataSource, IncomingMessageSuscription>
    {
        private static readonly Guid _suscriptionHandlerId = Guid.NewGuid();
        private static readonly DateTime _updateTime = new DateTime(2012, 2, 5, 7, 8, 9, 330);
        private string _messageType = "Test.Type";
        const string _handlerType = "this is a type";


        protected override string IdFieldName
        {
            get { return  IncomingMessageSuscription.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get { return IncomingMessageSuscription.TableName; }
        }

        protected override IncomingMessageSuscription GetExpectedWithChanges(IncomingMessageSuscription source)
        {
            source.BizMessageFullTypeName = "another";
            return source;
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            Guid cid;
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            dataAccessTestHelper.InsertAppComponent(LocalComponentId, LocalComponentId,0,false,false);

            return dataAccessTestHelper.InsertIncomingMessageSuscriptions(_messageType, _updateTime, LocalComponentId,
                                                     _suscriptionHandlerId,_handlerType);
        }


        protected override void CheckInsertedRecord(IncomingMessageSuscription record)
        {
            Assert.IsNotNull(record);
            Assert.IsTrue(record.SuscriptionHandlerId == _suscriptionHandlerId);
            Assert.AreEqual(_handlerType,record.HandlerType);
#if !NEED_FIX_MILLISECONDS
            Assert.IsTrue(record.DateLastUpdateUtc == _updateTime); //TODO:milliseconds are truncated
#endif
            Assert.AreEqual(_messageType, record.BizMessageFullTypeName);
        }

        protected override IncomingMessageSuscription GetExpected(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            AppComponent appComponent = dataAccessTestHelper.GetNewAppComponent();

            return new IncomingMessageSuscription
                       {
                           ComponentOwner = appComponent.ComponentId,
                           BizMessageFullTypeName = _messageType,
                           DateLastUpdateUtc = _updateTime,
                           SuscriptionHandlerId = _suscriptionHandlerId,
                           HandlerType = _handlerType
                       };
        }

        protected override IncomingMessageSuscriptionsDataSource GetDataSourceTarget(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            IDalSettings dataAccessSettings = dataAccessTestHelper.DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new IncomingMessageSuscriptionsDataSource(dataAccessSettings, LocalComponentId,dataAccessExecutor);
        }

       
    }
}