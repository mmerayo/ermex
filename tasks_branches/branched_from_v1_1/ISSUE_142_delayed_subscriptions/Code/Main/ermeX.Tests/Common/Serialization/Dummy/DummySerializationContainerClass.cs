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
using System.Linq;
using System.Text;

namespace ermeX.Tests.Common.Serialization.Dummy
{
    public class DummySerializationContainerClass
    {
        public DummySerializationContainerClass()
        {
            Data = new Dictionary<string, InnerClass>();
        }

        public Dictionary<string, InnerClass> Data { get; private set; }

        public void Add(string key, InnerClass item)
        {
            Data.Add(key,item);
        }

        public static bool operator ==(DummySerializationContainerClass a, DummySerializationContainerClass b)
        {
            if (ReferenceEquals(a, b))return true;
            if (((object)a == null) || ((object)b == null))return false;
            if (a.Data.Count != b.Data.Count)return false;

            return a.Data.Keys.All(key => b.Data.ContainsKey(key) && a.Data[key] == b.Data[key]);
        }
        public static bool operator !=(DummySerializationContainerClass a, DummySerializationContainerClass b)
        {
            return !(a == b);
        }

        public class InnerClass
        {
            public string TheString { get; set; }
            public Guid TheGuid { get; set; }
            public int TheInteger { get; set; }
            
            public static bool operator ==(InnerClass a, InnerClass b)
            {
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                if (((object)a == null) || ((object)b == null))
                {
                    return false;
                }
                return a.TheGuid == b.TheGuid && a.TheString == b.TheString && a.TheInteger == b.TheInteger;
            }
            public static bool operator !=(InnerClass a, InnerClass b)
            {
                return !(a == b);
            }

            public bool Equals(InnerClass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.TheString, TheString) && other.TheGuid.Equals(TheGuid) && other.TheInteger == TheInteger;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (InnerClass)) return false;
                return Equals((InnerClass) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = (TheString != null ? TheString.GetHashCode() : 0);
                    result = (result*397) ^ TheGuid.GetHashCode();
                    result = (result*397) ^ TheInteger;
                    return result;
                }
            }
        }

        public bool Equals(DummySerializationContainerClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Data, Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (DummySerializationContainerClass)) return false;
            return Equals((DummySerializationContainerClass) obj);
        }

        public override int GetHashCode()
        {
            return (Data != null ? Data.GetHashCode() : 0);
        }
    }
}
