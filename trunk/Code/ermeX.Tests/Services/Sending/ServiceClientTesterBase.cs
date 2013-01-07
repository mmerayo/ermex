// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using ermeX.Common;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Exceptions;
using ermeX.LayerMessages;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.Helpers;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.Services.Builtin.SuperSockets;
using ermeX.Tests.Services.Mock;

using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Tests.Services.Sending
{
    [Category(TestCategories.CoreUnitTest)]
    [TestFixture]
    internal abstract class ServiceClientTesterBase
    {
        //protected DummyTestServerBase<DummyDomainEntity> server = null;
        private static readonly MemoryCacheStore memoryCacheStore = new MemoryCacheStore(1000);

        protected static MemoryCacheStore CacheProvider
        {
            get { return memoryCacheStore; }
        }

        [TestFixtureSetUp]
        public void StartUp()
        {
            TestSettingsProvider.DropDatabases();
            IoCManager.Reset();
        }

        protected abstract MockTestServerBase<TMessage> GetDummyTestServer<TMessage>(bool local,
                                                                                     AutoResetEvent eventDone,
                                                                                     int freePort, Guid s);

        protected void DoSendMessageTest(MockTestServerBase<ServiceRequestMessage> server,
                                         ITransportSettings settings, Guid sId, int endpoint, string ip,
                                         DummyDomainEntity message, bool serverIsLocal = false)
        {
            server.ReceivedMessages.Clear();
            Assert.IsEmpty(server.ReceivedMessages);

            var lst = new List<ServerInfo> { new ServerInfo { Ip = ip, Port = endpoint, IsLocal = serverIsLocal, ServerId = sId } };

            DoSendMessageTest(server, settings, message, lst);
        }

        protected void DoSendMessageTest(MockTestServerBase<ServiceRequestMessage> s, ITransportSettings settings,
                                         DummyDomainEntity message,
                                         List<ServerInfo> serverInfos)
        {
            ServiceResult serviceResult;
            using (
                IEndPoint target = GetClient(CacheProvider, settings, serverInfos))
            {
                var layerMessage = LayerMessagesHelper.GetLayerMessage<DummyDomainEntity,TransportMessage>(LayerMessagesHelper.LayerMessageType.Transport, message);
                ServiceRequestMessage serviceRequestMessage = ServiceRequestMessage.GetForMessagePublishing(layerMessage);
                serviceResult = target.Send(serviceRequestMessage);
            }
            Assert.IsNotNull(serviceResult);
            Assert.IsTrue(serviceResult.Ok);
            
            Assert.IsNotEmpty(s.ReceivedMessages);
            var actual =(DummyDomainEntity) s.ReceivedMessages[0].Data.Data.Data.RawData;
            Assert.AreEqual(message.Id, actual.Id);

            Assert.IsNotNull(serviceResult.ServerMessages);
            Assert.AreEqual(serviceResult.ServerMessages[0],"AA");
        }

        protected abstract IEndPoint GetClient(MemoryCacheStore cacheProvider, ITransportSettings settings,
                                               List<ServerInfo> serverInfos);


        protected TestSettingsProvider.ClientSettings GetClientSettings()
        {
            return (TestSettingsProvider.ClientSettings)TestSettingsProvider.GetClientConfigurationSettingsSource();
        }

        protected void DoSendMessageTest2(MockTestServerBase<ChunkedServiceRequestMessage> s1,
                                          ITransportSettings settings, Guid sId, int endpoint, string ip,
                                          DummyDomainEntity message, bool serverIsLocal = false)
        {
            s1.ReceivedMessages.Clear();
            Assert.IsEmpty(s1.ReceivedMessages);

            var lst = new List<ServerInfo> { new ServerInfo { Ip = ip, Port = endpoint, IsLocal = serverIsLocal, ServerId = sId } };

            DoSendMessageTest2(s1, settings, message, lst);
        }

        protected void DoSendMessageTest2(MockTestServerBase<ChunkedServiceRequestMessage> s1,
                                          ITransportSettings settings, DummyDomainEntity message,
                                          List<ServerInfo> serverInfos)
        {
            ServiceResult serviceResult;
            using (
                IEndPoint target = GetClient(CacheProvider, settings, serverInfos))
            {
                var layerMessage = LayerMessagesHelper.GetLayerMessage<DummyDomainEntity, TransportMessage>(LayerMessagesHelper.LayerMessageType.Transport, message);
                serviceResult = target.Send(ServiceRequestMessage.GetForMessagePublishing(layerMessage));
            }
            Assert.IsNotNull(serviceResult);
            Assert.IsTrue(serviceResult.Ok);
            Assert.IsNotEmpty(s1.ReceivedMessages);

            Assert.AreEqual(s1.ReceivedMessages.Count,
                            s1.ReceivedMessages.Count(x => x.CorrelationId == s1.ReceivedMessages[0].CorrelationId));

            var receivedBytes = new List<byte>();

            foreach (ChunkedServiceRequestMessage chunk in s1.ReceivedMessages)
            {
                receivedBytes.AddRange( chunk.Data);
            }

            byte[] source = receivedBytes.ToArray();
            var received = (DummyDomainEntity)ObjectSerializer.DeserializeObject<ServiceRequestMessage>(source).Data.Data.Data.RawData;

            var expected = JsonSerializer.SerializeObjectToJson(message);

            var actual = JsonSerializer.SerializeObjectToJson(received);

            Assert.IsNotNull(actual);

            Assert.AreEqual(expected, actual);
        }

        private byte[] GenerateRandomBytes(int kbytes)
        {
            int bytes = kbytes * 1024;

            var result = new byte[bytes];

            var rnd = new Random();

            rnd.NextBytes(result);

            return result;
        }

        [Test]
        public void CanSendMessage()
        {
            Guid s = Guid.NewGuid();

            int freePort = new TestPort(10000, 11000);
            using (
                var server = GetDummyTestServer<ServiceRequestMessage>(false, null, freePort, s))
            {
                var expected = new DummyDomainEntity { Id = Guid.NewGuid() };

                DoSendMessageTest(server, GetClientSettings(), s, freePort,
                                  Networking.GetLocalhostIp(AddressFamily.InterNetwork), expected);
                server.Dispose();
            }
        }

       
        [Test]
        public void CanSendMessageWithLongResponse([Values(10, 50, 100, 200, 500, 1500, 5000, 10000,50000,100000)] int items)
        {
            if(items>200)
                Assert.Inconclusive("protobuf to support this or workaround boxing valuetypes");
            Guid s = Guid.NewGuid();

            int freePort = new TestPort(10000, 11000);
            AutoResetEvent eventDone = new AutoResetEvent(false);

            var response = new DummyDomainEntity() { Id = OperationIdentifiers.InternalMessagesOperationIdentifier, Dummies = new List<DummyDomainEntity>(items) };
            for (int i = 0; i < items; i++)
                response.Dummies.Add(new DummyDomainEntity() { Id = Guid.NewGuid() });

            List<DummySocketServerResult> lst = new List<DummySocketServerResult>() { new DummySocketServerResult(response.Id, response) };
            using (
                var server = new DummyTestSuperSocketServer<ServiceRequestMessage>(freePort, eventDone, lst))
            {

                var expected = new DummyDomainEntity { Id = Guid.NewGuid() };

                DoSendMessageTest(server, GetClientSettings(), s, freePort,
                                  Networking.GetLocalhostIp(AddressFamily.InterNetwork), expected);
                server.Dispose();
            }
        }

        [Test]
        public void CanSendMessageToLocalMachine()
        {
            Guid s = Guid.NewGuid();
            int freePort = new TestPort(10000, 11000);
            var eventDone = new AutoResetEvent(false);
            using (
                var server = GetDummyTestServer<ServiceRequestMessage>(true, eventDone,freePort, s))
            {
                var expected = new DummyDomainEntity { Id = Guid.NewGuid() };
                TestSettingsProvider.ClientSettings clientConfigurationSettings = GetClientSettings();

                clientConfigurationSettings.ComponentId = s;
                DoSendMessageTest(server, clientConfigurationSettings, s, freePort,
                                  Networking.GetLocalhostIp(AddressFamily.InterNetwork), expected, true);
            }
        }

        [Test(Description = "Tests to send a big file")]
        public void Can_Send_Chunked_Message([Values(1, 10, 25, 100, 256, 1024)] int mBytes)
        {
            if (mBytes > 10)
                Assert.Inconclusive("TODO avoid the out of memory exceptions");
            //Assert.Fail("Need to pass a memory profiler and optimize as this test is not working for " + mBytes);

            Guid s = Guid.NewGuid();

            var expected = new DummyDomainEntity { Id = Guid.NewGuid(), FileBytes = GenerateRandomBytes(mBytes * 1024) };

            int freePort = new TestPort(10000, 11000);
            var eventDone = new AutoResetEvent(false);

            using (
                MockTestServerBase<ChunkedServiceRequestMessage> s1 =
                    GetDummyTestServer<ChunkedServiceRequestMessage>(true, eventDone, freePort, s))
            {
                TestSettingsProvider.ClientSettings clientConfigurationSettings = GetClientSettings();

                clientConfigurationSettings.ComponentId = s;
                clientConfigurationSettings.MaxMessageKbBeforeChunking = 256;
                DoSendMessageTest2(s1, clientConfigurationSettings, s, freePort,
                                   Networking.GetLocalhostIp(AddressFamily.InterNetwork), expected, true);
            }
        }

        [Test(Description = "Tests to send a big file")]
        public void Can_Send_Huge_Not_Chunked_Message([Values(1, 5, 10, 15)] int mBytes)
        {
            if (mBytes > 10)
                Assert.Inconclusive("TODO");
            Guid s = Guid.NewGuid();

            var expected = new DummyDomainEntity { Id = Guid.NewGuid(), FileBytes = GenerateRandomBytes(mBytes * 1024) };

            int freePort = new TestPort(10000, 11000);
            var eventDone = new AutoResetEvent(false);

            using (var s1 = GetDummyTestServer<ServiceRequestMessage>(true, eventDone, freePort, s))
            {
                TestSettingsProvider.ClientSettings clientConfigurationSettings = GetClientSettings();

                clientConfigurationSettings.ComponentId = s;
                clientConfigurationSettings.MaxMessageKbBeforeChunking = int.MaxValue;
                DoSendMessageTest(s1, clientConfigurationSettings, s, freePort,
                                  Networking.GetLocalhostIp(AddressFamily.InterNetwork), expected, true);
            }
        }

        [Test]
        public void CantSendToWrongIp()
        {
            Guid s = Guid.NewGuid();
            int freePort = new TestPort(10000, 11000);
            using (
                MockTestServerBase<DummyDomainEntity> server = GetDummyTestServer<DummyDomainEntity>(false, null,
                                                                                                     freePort, s))
            {
                var expected = new DummyDomainEntity { Id = Guid.NewGuid() };


                var lst = new List<ServerInfo>
                              {
                                  new ServerInfo
                                      {Ip = "255.255.255.255", Port = freePort, IsLocal = false, ServerId = s}
                              };

                using (
                    IEndPoint target = GetClient(CacheProvider, GetClientSettings(), lst))
                {
                    var layerMessage = LayerMessagesHelper.GetLayerMessage<DummyDomainEntity, TransportMessage>(LayerMessagesHelper.LayerMessageType.Transport, expected);
                    Assert.Throws<ermeXComponentNotAvailableException>(() => target.Send(ServiceRequestMessage.GetForMessagePublishing(layerMessage)));
                    
                }
            }
        }

        [Test]
        public void ClientChoosesMostSuitableIpForSendingMessage([Values(true, false)] bool secondIsLocal)
        {
            Guid s = Guid.NewGuid();
            int freePort = new TestPort(10000, 11000);
            using (var server = GetDummyTestServer<ServiceRequestMessage>(secondIsLocal, null, freePort, s))
            {
                var expected = new DummyDomainEntity { Id = Guid.NewGuid() };

                var lst = new List<ServerInfo>
                              {
                                  new ServerInfo
                                      {Ip = "255.255.255.255", Port = freePort, IsLocal = false, ServerId = s},
                                  new ServerInfo
                                      {
                                          Ip = Networking.GetLocalhostIp(AddressFamily.InterNetwork),
                                          Port = freePort,
                                          IsLocal = secondIsLocal,
                                          ServerId = s
                                      }
                              };

                DoSendMessageTest(server, GetClientSettings(), expected, lst);
            }
        }

        [Ignore]
        [Test]
        public void Client_Reports_WhenServerCannotBeReached()
        {
            //all protocols
            throw new NotImplementedException();
        }


        [Ignore]
        [Test]
        public void Client_Reports_WhenTimeout()
        {
            //all protocols
            throw new NotImplementedException();
        }

        [Ignore]
        [Test]
        public void Client_Reports_When_Server_Failed_Handling()
        {
            //all protocols
            throw new NotImplementedException();
        }

        [Ignore]
        [Test]
        public void Client_Reports_When_Server_Handle_Success()
        {
            //all protocols
            throw new NotImplementedException();
        }
    }
}