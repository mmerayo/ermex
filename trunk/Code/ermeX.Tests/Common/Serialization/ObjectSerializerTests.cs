// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.IO;
using NUnit.Framework;
using ermeX.Common;
using ermeX.LayerMessages;
using ermeX.Tests.Common.Helpers;
using ermeX.Tests.Common.Serialization.Dummy;


namespace ermeX.Tests.Common.Serialization
{
    [TestFixture]
    internal class ObjectSerializerTests
    {
        //TODO: FORCE CONSTRUCTORS TO BE PRIVATE AND SHOULD WORK
        
        private string filePath = null;

        [TearDown]
        public void OnTearDown()
        {
            if(!string.IsNullOrEmpty(filePath))
            {
                File.Delete(filePath);
                filePath = null;
            }
        }

        [Test]
        public void CanSerializeAndDeserialize_ByteArray()
        {
            var expected = new DummySerializationEntity(2); //TODO: CREATE DEEPER
            var layerMessage = LayerMessagesHelper.GetLayerMessage<DummySerializationEntity,BizMessage>(LayerMessagesHelper.LayerMessageType.Biz, expected);
            byte[] byteExpected = ObjectSerializer.SerializeObjectToByteArray(layerMessage);

            var actualLayerMessage = ObjectSerializer.DeserializeObject<BizMessage>(byteExpected);
            Assert.AreEqual(JsonSerializer.SerializeObjectToJson(expected),actualLayerMessage.JsonMessage);
        }


        [Test]
        public void CanSerializeAndDeserialize_ToFile()
        {
            filePath = PathUtils.GetApplicationFolderPathFile("ObjectSerializerTests.test");
            if(File.Exists(filePath))
                File.Delete(filePath);
            var exp = LayerMessagesHelper.GetLayerMessage<DummySerializationEntity,BizMessage>( new DummySerializationEntity(2)); //TODO: CREATE DEEPER

            var expected = JsonSerializer.SerializeObjectToJson(exp);

            ObjectSerializer.SerializeObject(filePath, exp);
            var act = ObjectSerializer.DeserializeObject<BizMessage>(filePath);
            var actual = JsonSerializer.SerializeObjectToJson(act);
            Assert.AreEqual(expected, actual);

        }






    }
}