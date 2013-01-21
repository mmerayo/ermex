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
using System.Collections.Generic;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.Common.Serialization.Dummy
{
    //TODO: TYPES MUST HAVE PUBLIC SETTERS
    internal class DummySerializationEntity : DummySerializationEntityBase
    {
        //public DummySerializationEntity()
        //{
        //    //TODO: DUE TO CONSTRAINT IN SERIALIZER
        //}

        public DummySerializationEntity(int depth)
        {
            GenerateValueProperties();
            if (depth > 0)
                GenerateDepthProperties(depth - 1);
        }

        public DummySerializationEntity TheTypedValue { get; set; }
        public Dictionary<string, DummySerializationEntity> TheTypedDictionary { get; set; }
        public List<DummySerializationEntity> TheTypedList { get; set; }


        private void GenerateValueProperties()
        {
            TheInt = RandomHelper.GetRandomInt();
            TheString = RandomHelper.GetRandomString();
            TheEnumProperty = RandomHelper.GetRandomInt(0, 15000) % 2 == 1 ? TestEnum.Value1 : TestEnum.Value2;
        }

        private void GenerateDepthProperties(int nextLevelDepth)
        {
            TheObjectValue = new DummySerializationEntity(nextLevelDepth);
            TheBaseValue = new DummySerializationEntity(nextLevelDepth);
            TheTypedValue = new DummySerializationEntity(nextLevelDepth);

            TheObjectList = GenerateList<object>(nextLevelDepth);
            TheBaseList = GenerateList<DummySerializationEntityBase>(nextLevelDepth);
            TheTypedList = GenerateList<DummySerializationEntity>(nextLevelDepth);

            TheBaseDictionary = GenerateDictionary<DummySerializationEntityBase>(nextLevelDepth);
            TheObjectDictionary = GenerateDictionary<object>(nextLevelDepth);
            TheTypedDictionary = GenerateDictionary<DummySerializationEntity>(nextLevelDepth);
        }


        private List<TValue> GenerateList<TValue>(int nextLevelDepth) where TValue : class
        {
            var result = new List<TValue>();

            int numItems = RandomHelper.GetRandomInt(1, 15);


            for (int i = 0; i < numItems; i++)
                result.Add(new DummySerializationEntity(nextLevelDepth) as TValue);
            return result;
        }

        private Dictionary<string, TValue> GenerateDictionary<TValue>(int nextLevelDepth) where TValue : class
        {
            var result = new Dictionary<string, TValue>();
            int numItems = RandomHelper.GetRandomInt(1, 15);


            for (int i = 0; i < numItems; i++)
                result.Add(Guid.NewGuid().ToString(), new DummySerializationEntity(nextLevelDepth) as TValue);
            return result;
        }
    }
}