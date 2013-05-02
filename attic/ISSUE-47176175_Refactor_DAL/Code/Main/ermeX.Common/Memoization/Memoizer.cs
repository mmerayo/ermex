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
using System.Globalization;
using System.Linq;
using System.Text;

namespace ermeX.Common.Memoization
{
    public class Memoizer
    {
        public static Func<TReturn> Memoize<TReturn>(Func<TReturn> func)
        {
            object resultCache = null;
            return () =>
            {
                if (resultCache == null)
                    resultCache = func();

                return (TReturn)resultCache;
            };
        }
        public static Func<TSource, TReturn> Memoize<TSource, TReturn>(Func<TSource, TReturn> func)
        {
            var resultCache = new Dictionary<TSource, TReturn>();
            return s =>
            {
                if (!resultCache.ContainsKey(s))
                {
                    resultCache[s] = func(s);
                }
                return resultCache[s];
            };
        }
        public static Func<TSource1, TSource2, TReturn> Memoize<TSource1, TSource2, TReturn>(Func<TSource1, TSource2, TReturn> func)
        {
            var resultCache = new Dictionary<string, TReturn>();
            return (arg1, arg2) =>
            {
                var key = arg1.GetHashCode().ToString(CultureInfo.InvariantCulture) + arg2.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (!resultCache.ContainsKey(key))
                {
                    resultCache[key] = func(arg1, arg2);
                }
                return resultCache[key];
            };
        }


        public static Func<TSource1, TSource2, TSource3, TReturn> Memoize<TSource1, TSource2, TSource3, TReturn>(Func<TSource1, TSource2, TSource3, TReturn> func)
        {
            var resultCache = new Dictionary<string, TReturn>();
            return (arg1, arg2,arg3) =>
                {
                    var key = arg1.GetHashCode().ToString(CultureInfo.InvariantCulture) +
                              arg2.GetHashCode().ToString(CultureInfo.InvariantCulture) +
                              arg3.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (!resultCache.ContainsKey(key))
                {
                    resultCache[key] = func(arg1, arg2, arg3);
                }
                return resultCache[key];
            };
        }

        //TODO: extend on demand
    }
}
