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
using System.Text;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.Common.Serialization.Dummy
{
    internal abstract class DummySerializationEntityBase
    {
        public DummySerializationEntityBase TheBaseValue;
        private int _thePrivateField;

        protected DummySerializationEntityBase()
        {
            _thePrivateField = RandomHelper.GetRandomInt(12, 1000);
            ThePrivateProperty = RandomHelper.GetRandomInt(123, 11000);
            ThePrivateSetter = RandomHelper.GetRandomInt(33, 33333);
        }

        private int ThePrivateProperty { get; set; }
        public int ThePrivateSetter { get; private set; }

        public int TheInt { get; set; }
        public string TheString { get; set; }
        public object TheObjectValue { get; set; }
        public Dictionary<string, DummySerializationEntityBase> TheBaseDictionary { get; set; }
        public Dictionary<string, object> TheObjectDictionary { get; set; }
        public List<DummySerializationEntityBase> TheBaseList { get; set; }
        public List<object> TheObjectList { get; set; }

        public TestEnum TheEnumProperty { get; set; }


       

        #region Nested type: TestEnum

        internal enum TestEnum
        {
            Value1 = 1,
            Value2
        }

        #endregion
    }
}