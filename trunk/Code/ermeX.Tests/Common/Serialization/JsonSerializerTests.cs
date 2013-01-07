// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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