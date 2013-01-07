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
    internal class IncomingMessageDataSourceTester : DataSourceTesterBase<IncomingMessagesDataSource, IncomingMessage>
    {
        
        private readonly DateTime TimePublished = new DateTime(2010, 2, 3, 1, 2, 3, 330);
        private readonly DateTime TimeReceived = new DateTime(2010, 2, 3, 3, 4, 5, 330);
       
        private const int BMID = 2222;
        private const int BMID2 = 654978;

        private readonly Guid componentId = Guid.NewGuid();
        private Guid ownerCompId {get { return LocalComponentId; }}
        private readonly DummyDomainEntity message = new DummyDomainEntity {Id = Guid.NewGuid()};
        private readonly Guid suscriptionHandlerId = Guid.NewGuid();


        protected override string IdFieldName
        {
            get
            {
                return IncomingMessage.GetDbFieldName("Id"); }
        }

        protected override string TableName
        {
            get { return IncomingMessage.FinalTableName; }
        }
       

        protected override IncomingMessage GetExpectedWithChanges(IncomingMessage source)
        {
            source.BusMessageId = BMID2;
            return source;
        }

        protected override int InsertRecord(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            dataAccessTestHelper.InsertAppComponent(componentId, ownerCompId,0,false,false);

            return dataAccessTestHelper.InsertIncomingMessage(componentId, ownerCompId, BMID, 
                                         TimePublished, TimeReceived, suscriptionHandlerId);
        }

        protected override void CheckInsertedRecord(IncomingMessage record)
        {
            Assert.IsNotNull(record);
            Assert.IsTrue(record.PublishedTo == componentId);
            Assert.IsTrue(record.PublishedBy == ownerCompId);
            Assert.IsTrue(record.BusMessageId == BMID);
#if (!NEED_FIX_MILLISECONDS)
            Assert.IsTrue(record.TimePublishedUtc == TimePublished);
            Assert.IsTrue(record.TimeReceivedUtc == TimeReceived);

#endif
            Assert.IsTrue(record.ComponentOwner == ownerCompId);
            Assert.IsTrue(record.SuscriptionHandlerId == suscriptionHandlerId);
        }

        protected override IncomingMessage GetExpected(DbEngineType engine)
        {
            BusMessage busMessage = GetBusMessage(message);
            BusMessageData fromBusLayerMessage = BusMessageData.FromBusLayerMessage(LocalComponentId, busMessage, BusMessageData.BusMessageStatus.SenderSent);
            fromBusLayerMessage.Id = BMID;
            return new IncomingMessage(fromBusLayerMessage)
                       {
                           ComponentOwner = ownerCompId,
                           PublishedBy = ownerCompId,
                           PublishedTo = componentId,
                           TimePublishedUtc = TimePublished,
                           TimeReceivedUtc = TimeReceived,
                          
                           SuscriptionHandlerId = suscriptionHandlerId
                       };
        }

        private BusMessage GetBusMessage<TMessage>(TMessage data)
        {
            return new BusMessage(ownerCompId, new BizMessage(data));
        }


        protected override IncomingMessagesDataSource GetDataSourceTarget(DbEngineType engine)
        {
            DataAccessTestHelper dataAccessTestHelper = GetDataHelper(engine);
            IDalSettings dataAccessSettings =dataAccessTestHelper. DataAccessSettings;
            var dataAccessExecutor = new DataAccessExecutor(dataAccessSettings);
            BusMessageDataSource busMessageDataSource = GetBusMessageDataSource(dataAccessExecutor, dataAccessSettings);
            return new IncomingMessagesDataSource(busMessageDataSource,  dataAccessSettings, ownerCompId,dataAccessExecutor);
        }

        private BusMessageDataSource GetBusMessageDataSource(DbEngineType engineType)
        {
            var dataAccessExecutor = new DataAccessExecutor(GetDataHelper(engineType).DataAccessSettings);
            return new BusMessageDataSource(dataAccessExecutor.DalSettings, LocalComponentId, dataAccessExecutor);
        }

        protected BusMessageDataSource GetBusMessageDataSource(DataAccessExecutor dataAccessExecutor, IDalSettings dataAccessSettings)
        {
            return new BusMessageDataSource(dataAccessSettings,ownerCompId,dataAccessExecutor);
        }
    }
}