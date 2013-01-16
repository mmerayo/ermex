// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
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

        [Test]
        public void FromMarcGravell_Serialize_Deserialize_1M()
        {
            var rand = new Random(12345);
            for (int i = 0; i < 1000000; i++)
            {
                var obj = new TransportMessage(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), new BusMessage(
                    Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), new BizMessage(rand.Next())));
                var blob = ObjectSerializer.SerializeObjectToByteArray(obj);
                Assert.DoesNotThrow(()=>ObjectSerializer.DeserializeObject<TransportMessage>(blob));
            }
        }




    }
}