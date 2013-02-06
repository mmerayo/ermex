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
namespace ermeX.Common.Caching
{
    public interface ICacheProvider
    {
        int SecondsToExpire { get; set; }
        void Empty();
        T Get<T>(string key);
        void Add<T>(string key, T value);
        void Add<T>(string key, T value, int cacheDurationSeconds);
        void Remove(string key);
        bool Contains(string preselectedEndPointsKey);
        void ClearCache();
    }
}