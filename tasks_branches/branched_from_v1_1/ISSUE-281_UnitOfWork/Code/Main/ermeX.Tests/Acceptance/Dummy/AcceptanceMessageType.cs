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
using ermeX.Tests.Common;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.Acceptance.Dummy
{
    [Serializable]
    public abstract class AcceptanceMessageType : MarshalByRefObject
    {

        public AcceptanceMessageType(bool generateRandomValues = false)
        {
            if (generateRandomValues)
                GenerateRandomValues();
        }
        public Guid Id { get; set; }
        public int TheInt { get; set; }
        public string TheString { get; set; }
        public int[] TheArray { get; set; }
        public List<string> TheList { get; set; }
        public DateTime TheDateTime { get; set; }

        public void GenerateRandomValues()
        {
            TheInt = RandomHelper.GetRandomInt();
            TheString = RandomHelper.GetRandomString(15);

            TheArray = new int[RandomHelper.GetRandomInt(100)];
            for (int i = 0; i < TheArray.Length; i++)
                TheArray[i] = RandomHelper.GetRandomInt(0, 255);

            int items = RandomHelper.GetRandomInt(1);
            TheList = new List<string>(items);
            for (int i = 0; i < items; i++)
                TheList.Add(RandomHelper.GetRandomString());

            TheDateTime = RandomHelper.GetRandomDateTime();

        }


        protected bool CompareLists(AcceptanceMessageType other)
        {
            if ((this.TheArray == null && other.TheArray != null) || (this.TheArray != null && other.TheArray == null))
                return false;
            if ((this.TheList == null && other.TheList != null) || (this.TheList != null && other.TheList == null))
                return false;
            if (TheArray != null && TheArray.Length != other.TheArray.Length)
                return false;
            if (TheList != null && this.TheList.Count != other.TheList.Count)
                return false;

            if (TheArray != null)
                for (int index = 0; index < TheArray.Length; index++)
                {
                    if (TheArray[index] != other.TheArray[index])
                        return false;
                }

            if (TheList != null)
                for (int index = 0; index < TheList.Count; index++)
                {
                    if (TheList[index] != other.TheList[index])
                        return false;
                }
            return true;
        }



    }
}