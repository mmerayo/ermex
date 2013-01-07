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
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;

namespace ermeX.Tests.DAL.Integration.DataSources
{
    //[TestFixture]
    internal class OutgoingMessageDataSourceTester : DataSourceTesterBase<OutgoingMessagesDataSource, OutgoingMessage>
    {
       
        private readonly DateTime TimePublished = new DateTime(2010, 2, 3, 1, 2, 3, 330);
        private int Tries = 6;
        private bool Errored;
        
        private readonly Guid componentId = Guid.NewGuid();
        private readonly DummyDomainEntity message = new DummyDomainEntity {Id = Guid.NewGuid()};
        private const int BMID = 2222;
        private const int BMID2 = 654978;

        protected override string IdFieldName
        {
            get { return OutgoingMessage.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get
            {
                return OutgoingMessage.FinalTableName;
            }
        }

       

        protected override OutgoingMessage GetExpectedWithChanges(OutgoingMessage source)
        {
            source.BusMessageId = BMID2;
            return source;
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            dataAccessTestHelper.InsertAppComponent(componentId, LocalComponentId,0,false,false);

            return dataAccessTestHelper.InsertOutgoingMessage(componentId, LocalComponentId, BMID, 
                                         TimePublished, Tries, Errored);
        }

        protected override void CheckInsertedRecord(OutgoingMessage record)
        {
            Assert.IsNotNull(record);
            Assert.IsTrue(record.PublishedTo == componentId);
            Assert.IsTrue(record.PublishedBy == LocalComponentId);
            Assert.IsTrue(record.BusMessageId == BMID);
#if (!NEED_FIX_MILLISECONDS)
            Assert.IsTrue(record.TimePublishedUtc == TimePublished);
#endif
            Assert.IsTrue(record.ComponentOwner == LocalComponentId);
            Assert.IsTrue(record.Tries == Tries);
        }

        protected override OutgoingMessage GetExpected(DbEngineType engine)
        {
            BusMessageData fromBusLayerMessage = BusMessageData.FromBusLayerMessage(LocalComponentId, GetBusMessage(message), BusMessageData.BusMessageStatus.ReceiverDispatchable);
            fromBusLayerMessage.Id = BMID;
            return new OutgoingMessage (fromBusLayerMessage)
                       {
                           ComponentOwner = LocalComponentId,
                           Failed = Errored,
                           PublishedBy = LocalComponentId,
                           PublishedTo = componentId,
                           TimePublishedUtc = TimePublished,
                           Tries = Tries
                       };
        }

        private BusMessage GetBusMessage<TMessage>(TMessage data)
        {
            return new BusMessage(LocalComponentId,new BizMessage(data));
        }

        protected override OutgoingMessagesDataSource GetDataSourceTarget(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            IDalSettings dataAccessSettings = dataAccessTestHelper.DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            return new OutgoingMessagesDataSource(dataAccessSettings, LocalComponentId, dataAccessExecutor);
        }
    }
}