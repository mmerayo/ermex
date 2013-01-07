// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ermeX.Common;
using ermeX.LayerMessages;
using ermeX.Tests.Common.Helpers;
using ermeX.Tests.Common.RandomValues;
using ermeX.Tests.Common.Serialization.Dummy;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Tests.Common.Serialization
{
    [TestFixture]
    public class TestApplicationSerializableMessages
    {
        private string FileToDelete { get; set; }

        [SetUp]
        public void SetUp()
        {
            FileToDelete = null;            
        }

        [TearDown]
        public void TearDown()
        {
            if(!string.IsNullOrEmpty(FileToDelete))
                File.Delete(FileToDelete);
        }

       

        [Test]
        public void CanSerializeAndDeserialize_LayerMessage_ToFile([Values(LayerMessagesHelper.LayerMessageType.Biz,LayerMessagesHelper.LayerMessageType.Bus,LayerMessagesHelper.LayerMessageType.Transport)] LayerMessagesHelper.LayerMessageType messageType)
        {
            FileToDelete = PathUtils.GetApplicationFolderPathFile("TestFile.test");
            if(File.Exists(FileToDelete))
                File.Delete(FileToDelete);

            var data = new DummySerializationEntity(2); //TODO: CREATE DEEPER

            var bizMessage = LayerMessagesHelper.GetLayerMessage(messageType, data);

            ObjectSerializer.SerializeObject(FileToDelete, bizMessage);

            DummySerializationEntity current;
            object deserializedObject;
            switch(messageType)
            {
                case LayerMessagesHelper.LayerMessageType.Biz:
                    deserializedObject = ObjectSerializer.DeserializeObject<BizMessage>(FileToDelete);
                    current = (DummySerializationEntity)((BizMessage)deserializedObject).RawData;
                    break;
                case LayerMessagesHelper.LayerMessageType.Bus:
                    deserializedObject = ObjectSerializer.DeserializeObject<BusMessage>(FileToDelete);
                    current =(DummySerializationEntity)((BusMessage) deserializedObject).Data.RawData;
                    break;
                case LayerMessagesHelper.LayerMessageType.Transport:
                    deserializedObject = ObjectSerializer.DeserializeObject<TransportMessage>(FileToDelete);
                    current = (DummySerializationEntity)((TransportMessage) deserializedObject).Data.Data.RawData;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("messageType");
            }

            Assert.AreEqual(JsonSerializer.SerializeObjectToJson(bizMessage), JsonSerializer.SerializeObjectToJson(deserializedObject));

            string actual = JsonSerializer.SerializeObjectToJson(current);
            string expected = JsonSerializer.SerializeObjectToJson(data);
            Assert.AreEqual(expected,actual, string.Format("expected:{1}{0}*** actual:{2}{0}",Environment.NewLine, expected,actual));
        }

        [Test]
        public void CanSerializeAndDeserialize_ChunckedServiceRequest_ToFile()
        {
            FileToDelete = PathUtils.GetApplicationFolderPathFile("TestFile.test");
            if (File.Exists(FileToDelete))
                File.Delete(FileToDelete);

            var data = new ChunkedServiceRequestMessage(Guid.NewGuid(), RandomHelper.GetRandomInt(), false,
                                                        Guid.NewGuid(), new byte[]{23,55});


            ObjectSerializer.SerializeObject(FileToDelete, data);

            var deserializedObject = ObjectSerializer.DeserializeObject<ChunkedServiceRequestMessage>(FileToDelete);
            

            Assert.AreEqual(JsonSerializer.SerializeObjectToJson(data), JsonSerializer.SerializeObjectToJson(deserializedObject));
        }

        [Test]
        public void CanSerializeAndDeserialize_ServiceRequest_ToFile()
        {
            FileToDelete = PathUtils.GetApplicationFolderPathFile("TestFile.test");
            if (File.Exists(FileToDelete))
                File.Delete(FileToDelete);

            var data = new DummySerializationEntity(2); //TODO: CREATE DEEPER

            object layerMessage = LayerMessagesHelper.GetLayerMessage(LayerMessagesHelper.LayerMessageType.Transport, data);
            var serviceRequestMessage = ServiceRequestMessage.GetForMessagePublishing((TransportMessage)layerMessage);

            ObjectSerializer.SerializeObject(FileToDelete, serviceRequestMessage);

            var deserializedObject = ObjectSerializer.DeserializeObject<ServiceRequestMessage>(FileToDelete);
            var current = (DummySerializationEntity) deserializedObject.Data.Data.Data.RawData;
            
            Assert.AreEqual(JsonSerializer.SerializeObjectToJson(serviceRequestMessage), JsonSerializer.SerializeObjectToJson(deserializedObject));

            string actual = JsonSerializer.SerializeObjectToJson(current);
            string expected = JsonSerializer.SerializeObjectToJson(data);
            Assert.AreEqual(expected, actual, string.Format("expected:{1}{0}*** actual:{2}{0}", Environment.NewLine, expected, actual));
        }

        [Test]
        public void CanSerializeAndDeserialize_ServiceRequest_WithCollection([Values(1,2,40,100,200,500,1000,5000,10000,50000,500000)] int numItems)
        {
            if (numItems > 200)
                Assert.Inconclusive("protobuf to support this or workaround boxing valuetypes");
            FileToDelete = PathUtils.GetApplicationFolderPathFile("TestFile.test");
            if (File.Exists(FileToDelete))
                File.Delete(FileToDelete);

            var data = new List<DummySerializationEntity>(numItems);

            for(int i=0;i<numItems;i++)
                data.Add(new DummySerializationEntity(1)); 

            object layerMessage = LayerMessagesHelper.GetLayerMessage(LayerMessagesHelper.LayerMessageType.Transport, data);
            var serviceRequestMessage = ServiceRequestMessage.GetForMessagePublishing((TransportMessage)layerMessage);

            ObjectSerializer.SerializeObject(FileToDelete, serviceRequestMessage);

            var deserializedObject = ObjectSerializer.DeserializeObject<ServiceRequestMessage>(FileToDelete);
            var current = (List<DummySerializationEntity>)deserializedObject.Data.Data.Data.RawData;

            Assert.AreEqual(JsonSerializer.SerializeObjectToJson(serviceRequestMessage), JsonSerializer.SerializeObjectToJson(deserializedObject));

            string actual = JsonSerializer.SerializeObjectToJson(current);
            string expected = JsonSerializer.SerializeObjectToJson(data);
            Assert.AreEqual(expected, actual, string.Format("expected:{1}{0}*** actual:{2}{0}", Environment.NewLine, expected, actual));
        }

        [Test]
        public void CanSerializeAndDeserialize_ServiceResult_ToFile()
        {
            FileToDelete = PathUtils.GetApplicationFolderPathFile("TestFile.test");
            if (File.Exists(FileToDelete))
                File.Delete(FileToDelete);

            var data = new DummySerializationEntity(2); //TODO: CREATE DEEPER

            var message = new ServiceResult(true) {AsyncResponseId = Guid.NewGuid(), ResultData = data};
            message.ServerMessages.Add("message 1");
            message.ServerMessages.Add("message 2");

            ObjectSerializer.SerializeObject(FileToDelete, message);

            var deserializedObject = ObjectSerializer.DeserializeObject<ServiceResult>(FileToDelete);
            var current = (DummySerializationEntity)deserializedObject.ResultData;

            Assert.AreEqual(JsonSerializer.SerializeObjectToJson(message), JsonSerializer.SerializeObjectToJson(deserializedObject));

            string actual = JsonSerializer.SerializeObjectToJson(current);
            string expected = JsonSerializer.SerializeObjectToJson(data);
            Assert.AreEqual(expected, actual, string.Format("expected:{1}{0}*** actual:{2}{0}", Environment.NewLine, expected, actual));

            CollectionAssert.AreEqual(message.ServerMessages, deserializedObject.ServerMessages);
            
        }
        
        [Test]
        public void CanSerializeAndDeserialize_BizMessage_and_Modify_TheProtobufContract()
        {
            FileToDelete = PathUtils.GetApplicationFolderPathFile("TestFile.test");
            if (File.Exists(FileToDelete))
                File.Delete(FileToDelete);

            var data = new DummySerializationEntity(2); //TODO: CREATE DEEPER
            object layerMessage = LayerMessagesHelper.GetLayerMessage(LayerMessagesHelper.LayerMessageType.Biz, data);
            ObjectSerializer.SerializeObject(FileToDelete,layerMessage);
            File.Delete(FileToDelete);
            var data2 = 150;
            object layerMessage2 = LayerMessagesHelper.GetLayerMessage(LayerMessagesHelper.LayerMessageType.Biz, data2);
            ObjectSerializer.SerializeObject(FileToDelete, layerMessage2);

        }
    }
}
