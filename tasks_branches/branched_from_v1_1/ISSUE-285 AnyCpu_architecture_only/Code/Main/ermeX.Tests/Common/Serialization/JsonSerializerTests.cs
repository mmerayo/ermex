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
using NUnit.Framework;
using ermeX.Common;
using ermeX.Tests.Common.RandomValues;
using ermeX.Tests.Common.Serialization.Dummy;

namespace ermeX.Tests.Common.Serialization
{
    [TestFixture]
    internal class JsonSerializerTests
    {

        [Test]
        public void CanSerializeAndDeserialize_Json()
        {
            var expected = new DummySerializationEntity(3);

            string jsonExpected = JsonSerializer.SerializeObjectToJson(expected);

            var actual = JsonSerializer.DeserializeObjectFromJson<DummySerializationEntity>(jsonExpected);
            string jsonActual = JsonSerializer.SerializeObjectToJson(actual);

            Assert.AreEqual(jsonExpected, jsonActual);
        }

        [Test]
        public void CanSerializeAndDeserialize_ClassWithInnerClass_Json()
        {
            var expected = new DummySerializationContainerClass();
            const string key1 = "k1";
            var innerClass1 = new DummySerializationContainerClass.InnerClass()
                                  {
                                      TheGuid = Guid.NewGuid(), TheString = RandomHelper.GetRandomString(), TheInteger = RandomHelper.GetRandomInt()
                                  };
            const string key2 = "k2";
            var innerClass2 = new DummySerializationContainerClass.InnerClass()
                                  {
                                      TheGuid = Guid.NewGuid(),
                                      TheString = RandomHelper.GetRandomString(),
                                      TheInteger = RandomHelper.GetRandomInt()
                                  };
            expected.Add(key1,innerClass1);
            expected.Add(key2, innerClass2);

            string jsonExpected = JsonSerializer.SerializeObjectToJson(expected);

            var actual = JsonSerializer.DeserializeObjectFromJson<DummySerializationContainerClass>(jsonExpected);
            string jsonActual = JsonSerializer.SerializeObjectToJson(actual);

            Assert.AreEqual(jsonExpected, jsonActual);
            Assert.IsTrue(expected== actual);
        }
    }
}