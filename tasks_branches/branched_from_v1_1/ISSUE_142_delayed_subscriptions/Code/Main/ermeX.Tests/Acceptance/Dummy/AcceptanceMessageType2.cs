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
using ermeX.Tests.Common;

namespace ermeX.Tests.Acceptance.Dummy
{
    [Serializable]
    public class AcceptanceMessageType2 : AcceptanceMessageType
    {


        public AcceptanceMessageType2(bool generateRandomValues = false)
            : base(generateRandomValues)
        {

        }

        public static bool operator ==(AcceptanceMessageType2 a, AcceptanceMessageType2 b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            if (a.GetType() != b.GetType())
                throw new InvalidOperationException("Types are different");

            // Return true if the fields match:
            return a.Id == b.Id && a.TheInt == b.TheInt && a.TheString == b.TheString && a.CompareLists(b) && a.TheDateTime.Ticks == b.TheDateTime.Ticks;
        }

        public static bool operator !=(AcceptanceMessageType2 a, AcceptanceMessageType2 b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(AcceptanceMessageType2)) return false;
            return this == (AcceptanceMessageType2)obj;
        }
        public bool Equals(AcceptanceMessageType2 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this == other;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int result = TheInt;
                result = (result * 397) ^ (TheString != null ? TheString.GetHashCode() : 0);
                result = (result * 397) ^ (TheArray != null ? TheArray.GetHashCode() : 0);
                result = (result * 397) ^ (TheList != null ? TheList.GetHashCode() : 0);
                return result;
            }
        }

    }
}
