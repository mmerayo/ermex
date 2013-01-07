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
using ermeX.LayerMessages;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //TODO: test al possible operations in all datasources

    //[TestFixture]
    internal class BusMessageDataSourceTester :
        DataSourceTesterBase<BusMessageDataSource, BusMessageData>
    {
        private readonly DateTime VersionUtc = DateTime.UtcNow;
        private readonly Guid MessageId = Guid.NewGuid();
        private readonly Guid Publisher = Guid.NewGuid();

        private readonly DateTime CreatedTimeUtc = DateTime.Now.AddYears(5);
        private const string JsonMessage = "{\"title\": \"example\"}";
        private const BusMessageData.BusMessageStatus ExpectedStatus = BusMessageData.BusMessageStatus.SenderDispatchPending;

        protected override string IdFieldName
        {
            get
            {
                return BusMessageData.GetDbFieldName("Id");
            }
        }

        protected override string TableName
        {
            get { return BusMessageData.TableName; }
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            return GetDataHelper(engine).InsertBusMessage(OwnerComponentId, OwnerComponentId, VersionUtc, MessageId,Publisher,CreatedTimeUtc,JsonMessage, (int)ExpectedStatus);
        }

        protected override void CheckInsertedRecord(BusMessageData record)
        {
            Assert.IsNotNull(record);
            Assert.AreEqual(record.CreatedTimeUtc, CreatedTimeUtc);
            Assert.AreEqual(record.JsonMessage, JsonMessage);
            Assert.AreEqual(record.MessageId, MessageId);
            Assert.AreEqual(record.Publisher, Publisher);
        }


        protected override BusMessageData GetExpected(DbEngineType engine)
        {
            return new BusMessageData(MessageId,CreatedTimeUtc,Publisher,JsonMessage,ExpectedStatus)
                       {
                           ComponentOwner = OwnerComponentId,
                           //Version=VersionUtc.Ticks
                       };
        }


        protected override BusMessageData GetExpectedWithChanges(BusMessageData source)
        {
            source.MessageId = Guid.NewGuid();
            return source;
        }


        protected override BusMessageDataSource GetDataSourceTarget(DbEngineType engine)
        {
            IDalSettings dataAccessSettings =GetDataHelper(engine).DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new BusMessageDataSource(dataAccessSettings, OwnerComponentId,dataAccessExecutor);
        }


      

    
        private Guid OwnerComponentId {
            get { return LocalComponentId; }
        }
         
      
  
    }
}