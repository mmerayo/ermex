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

namespace Common.Base
{
    public abstract class DataSource<TKey,TData>
    {

        public event EventHandler CollectionChanged;

        public void OnCollectionChanged(EventArgs e)
        {
            EventHandler handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        protected readonly Dictionary<TKey, TData> Source = new Dictionary<TKey, TData>();

        protected abstract TKey GetKey(TData item);

        /// <summary>
        /// Saves an item
        /// </summary>
        /// <param name="item"></param>
        public void Save(TData item)
        {
            if (item.Equals(default(TData)))
                throw new ArgumentNullException("item");

            var key = GetKey(item);
            if (key.Equals(default(TKey)))
                throw new InvalidOperationException("Invalid key");

            lock (Source)
            {
                if (Source.ContainsKey(key))
                    Source[key] = item;
                else
                    Source.Add(key, item);
                OnCollectionChanged(null);
            }
        }

        /// <summary>
        /// gets an item from the collection
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TData Get(TKey key)
        {
            lock (Source)
                return Source.ContainsKey(key) ? Source[key] : default(TData);
        }

        /// <summary>
        /// deletes an item from the source given its key
        /// </summary>
        /// <param name="key"></param>
        public void Delete(TKey key)
        {
            lock (Source)
                if(Source.ContainsKey(key))
                {
                    Source.Remove(key);
                    OnCollectionChanged(null);
                }
        }

        /// <summary>
        /// Gets the current data stored
        /// </summary>
        public List<TData> Data
        {
            get
            {
                lock(Source)
                {
                    return new List<TData>(Source.Values);
                }
            }
        }

    }
}