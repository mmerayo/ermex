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
using System.Text;

namespace ermeX.Tests.Common.RandomValues
{
    public static class RandomHelper
    {
        public static string GetRandomString(int maxLenght = 5)
        {
            var builder = new StringBuilder();
            var random = new Random();
            char ch;
            int size = GetRandomInt(1, maxLenght);
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static int GetRandomInt(int minValue = 0, int maxValue = 20)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            return random.Next(minValue, maxValue<int.MaxValue? maxValue+1:int.MaxValue);
        }
        public static int GetRandomInt()
        {
            return GetRandomInt(0, int.MaxValue);
        }

        public static DateTime GetRandomDateTime()
        {
            var to = DateTime.Now;
            var from = new DateTime(1970, 1, 1);
            var range = new TimeSpan(to.Ticks - from.Ticks);
            var random = new Random((int)DateTime.Now.Ticks);
            return from + new TimeSpan((long)(range.Ticks * random.NextDouble()));
        }
    }
}
