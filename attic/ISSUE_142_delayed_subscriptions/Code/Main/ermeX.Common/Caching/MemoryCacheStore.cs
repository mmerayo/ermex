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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;

namespace ermeX.Common.Caching
{
    internal class MemoryCacheStore : ICacheProvider
    {
        private readonly object SyncLocker = new object();
        private IDictionary<string, ICachedItem> _cache = new ConcurrentDictionary<string, ICachedItem>();

        private int _secondsToExpire;

        public MemoryCacheStore(int secondsToExpire)
        {
            SecondsToExpire = secondsToExpire;
        }

        #region ICacheProvider Members

        public int SecondsToExpire
        {
            get { return _secondsToExpire; }
            set
            {
                if (value < 0)
                    throw new InvalidOperationException("SecondsToExpire must be a value >=0");
                _secondsToExpire = value;
            }
        }


        public void Empty()
        {
            lock (SyncLocker)
            {
                _cache.Clear();
                _cache = new Dictionary<string, ICachedItem>();
            }
        }

        public T Get<T>(string key)
        {
            T result = default(T);
            lock (SyncLocker)
            {
                if (_cache.ContainsKey(key))
                    result = (CachedItem<T>) _cache[key];
            }
            return result;
        }

        public void Add<T>(string key, T value)
        {
            Add(key, value, SecondsToExpire);
        }

        public void Add<T>(string key, T value, int cacheDurationSeconds)
        {
            Type type = typeof (T);

            lock (SyncLocker)
            {
                var cachedItem = new CachedItem<T>(key, value, cacheDurationSeconds);

                if (!_cache.ContainsKey(key))
                {
                    _cache.Add(key, cachedItem);
                }
                else
                {
                    _cache[key] = cachedItem;
                }
                cachedItem.MustExpireNow += cachedItem_MustExpireNow;
            }
        }

        public void Remove(string key)
        {
            lock (SyncLocker)
            {
                if (!_cache.ContainsKey(key))
                    throw new ApplicationException(String.Format("An object with key '{0}' does not exists in cache",
                                                                 key));
                var item = _cache[key];

                _cache.Remove(key);
                item.MustExpireNow -= cachedItem_MustExpireNow;
                item.Dispose();
            }
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        public void ClearCache()
        {
            lock (SyncLocker)
            {
                _cache.Clear();
            }
        }

        #endregion

        private void cachedItem_MustExpireNow(ICachedItem itemToExpire)
        {
            lock (SyncLocker)
            {
                Remove(itemToExpire);
            }
        }

        private void Remove(ICachedItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            lock (SyncLocker)
            {
                Remove(item.Key);
            }
        }

        #region Nested type: CachedItem

        private sealed class CachedItem<T> : ICachedItem
        {
            private readonly int _cacheDurationSeconds;
            private readonly object _expiredLocker = new object();

            private readonly Timer _timerEvent;
            private bool _expired;
            private bool disposed;

            public CachedItem(string key, T item, int cacheDurationSeconds)
            {
                _cacheDurationSeconds = cacheDurationSeconds;
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("key is not valid");
                Key = key;
                Item = item;

                if (cacheDurationSeconds > 0)
                {
                    _timerEvent = new Timer(cacheDurationSeconds*1000) {AutoReset = false};
                    _timerEvent.Elapsed += timerEvent_Elapsed;
                    _timerEvent.Start();
                }
            }

            public T Item { get; private set; }

            #region ICachedItem Members

            public event MustExpireHandler MustExpireNow;


            public void Expire()
            {
                OnMustExpireNow();
            }

            public string Key { get; private set; }

            object ICachedItem.Item
            {
                get { return Item; }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            private void OnMustExpireNow()
            {
                if (_cacheDurationSeconds > 0 && MustExpireNow != null)
                    lock (_expiredLocker)
                        if (!_expired)
                        {
                            _timerEvent.Stop();
                            MustExpireNow(this);
                            _expired = true;
                        }
            }

            private void timerEvent_Elapsed(object sender, ElapsedEventArgs e)
            {
                OnMustExpireNow();
            }

            public static implicit operator T(CachedItem<T> a)
            {
                return a.Item;
            }


            private void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        _timerEvent.Stop();
                        _timerEvent.Dispose();
                    }
                }
                disposed = true;
            }
        }

        #endregion

        #region Nested type: ICachedItem

        private interface ICachedItem : IDisposable
        {
            string Key { get; }
            object Item { get; }
            event MustExpireHandler MustExpireNow;
            void Expire();
        }

        #endregion

        #region Nested type: MustExpireHandler

        private delegate void MustExpireHandler(ICachedItem itemToExpire);

        #endregion
    }
}