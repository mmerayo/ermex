using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.DAL.DataAccess.DataSources;
using ermeX.LayerMessages;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Threading.Queues;

namespace ermeX.Tests.Bus.Publishing.Dispatching.Messages
{

    sealed class MessageCollectorTester : DataAccessTestBase
    {
        private SystemTaskQueue _systemQueue=new SystemTaskQueue();

        private MessageCollector GetInstance(DbEngineType dbEngine,Action<MessageDistributor.MessageDistributorMessage> messageReceived, out IMessageDistributor mockedDistributor)
        {
            var settings = TestSettingsProvider.GetClientConfigurationSettingsSource();
            var busMessageDataSource = GetBusMessageDataSource(dbEngine);
            var outgoingDataSource = GetOutgoingMessageDataSource(dbEngine);
            var mock = new Mock<IMessageDistributor>();
            mock.Setup(x=>x.EnqueueItem(It.IsAny<MessageDistributor.MessageDistributorMessage>())).Callback(messageReceived);
            mockedDistributor = mock.Object;
            return new MessageCollector(settings, busMessageDataSource, _systemQueue, outgoingDataSource,mockedDistributor);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanDispatchMessage(DbEngineType dbEngine )
        {
            IMessageDistributor mockedDistributor;
            var actual = new List<MessageDistributor.MessageDistributorMessage>();
            var expected = new BusMessage(Guid.NewGuid(), DateTime.UtcNow, LocalComponentId,
                                         new BizMessage("theData"));
            using (var target = GetInstance(dbEngine, actual.Add, out mockedDistributor))
                target.Dispatch(expected);
           
            Assert.IsTrue(actual.Count==1);
            Assert.AreSame(expected, actual[0]);
            //TODO: check db data
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void RemovesExpiredItems(DbEngineType dbEngine)
        {
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void SendsExistingItemsOnStart(DbEngineType dbEngine)
        {
            throw new NotImplementedException();
        }

    }
}
