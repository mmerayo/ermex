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
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using ermeX.Common;
using ermeX.Common.Caching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Exceptions;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.Reception;

namespace ermeX.Transport.Publish
{
    internal abstract class ServiceClientBase<TClient> : IEndPoint where TClient : class, IDisposable
    {
        protected ServerInfo _server;

        protected ICacheProvider CacheProvider { get; set; }
        protected ITransportSettings Settings { get; set; }

        private string CacheKey
        {
            get { return string.Format("{0}_Sc_{1}", Settings.ComponentId, _server.ServerId); }
        }

        protected List<ServerInfo> ServerInfos { get; set; }

        #region IEndPoint Members


        public ServiceResult Send(ServiceRequestMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            ServiceResult result = null;

            SetClient<TClient>(null);
            foreach (var sInfo in ServerInfos)
            {
                _server = sInfo;
                using (TClient client = GetClientInstance())
                {
                    if (client == null)
                        continue;
                    message.ServerId = _server.ServerId;
                    try
                    {
                        result = DoSend(message, client);
                    }
                    catch (ermeXComponentNotAvailableException)
                    {
                        continue;
                    }
                    //catch(Exception ex)
                    //{
                    //    Log
                    //    continue
                    //}
                    if (result.Ok)
                        break;
                }
            }


            if (result == null)
            {
                throw new ermeXComponentNotAvailableException(message.ServerId);
            }

            return result;
        }

        #endregion

        private TClient GetClient<TClient>()
        {
            if (CacheProvider.Contains(CacheKey))
                return CacheProvider.Get<TClient>(CacheKey);
            return default(TClient);
        }

        private void SetClient<TClient>(TClient client)
        {
            CacheProvider.Add(CacheKey, client);
        }

        


        protected abstract TClient GetClientInstance();

        protected abstract void SetUpCurrentServer();

        protected abstract ServiceResult DoSend(ServiceRequestMessage message, TClient client);

        protected ServiceResult DoSend(ServiceRequestMessage message, object proxy)
        {
            if (message == null)throw new ArgumentNullException("message");
            if (proxy == null) throw new ArgumentNullException("proxy");

            ServiceResult result = null;
            byte[] byteRes = null;

            var toSend = ObjectSerializer.SerializeObjectToByteArray(message);

            try
            {
                if (toSend.Length/1024 <= Settings.MaxMessageKbBeforeChunking)
                {
                    byteRes = DoConcreteSend(proxy, toSend);
                }
                else
                {
                    var chunks = GetChunks(toSend, ((toSend.Length/1024)/Settings.MaxMessageKbBeforeChunking) + 1);

                    var chunkBytes = new List<byte[]>();
                    foreach (var chunk in chunks)
                    {
                        var bytes = ObjectSerializer.SerializeObjectToByteArray(chunk);
                        chunkBytes.Add(bytes);
                    }
                    byteRes = DoConcreteSend(proxy, chunkBytes);
                }
                result = ObjectSerializer.DeserializeObject<ServiceResult>(byteRes);
            }
            catch (SocketException ex)
            {
                throw new ermeXComponentNotAvailableException(message.ServerId);
            }
            catch (Exception ex)
            {
                result = new ServiceResult(false);
                result.ServerMessages.Add(string.Format("{0} {1}", ex,
                                                        ex.InnerException != null
                                                            ? ex.InnerException.Message
                                                            : string.Empty));
            }
            return result;
        }


        protected List<ChunkedServiceRequestMessage> GetChunks(byte[] source, int chunksNumber)
        {
            var result = new List<ChunkedServiceRequestMessage>(chunksNumber);

            var correlationId = Guid.NewGuid();

            var len = source.Length/chunksNumber;

            for (int i = 0; i < chunksNumber; i++)
            {
                var lastChunk = i == chunksNumber - 1;
                var subArray = lastChunk ? source.SubArray(i*len) : source.SubArray(i*len, len);
                result.Add(new ChunkedServiceRequestMessage(correlationId, i, lastChunk,
                                                            ServerBase.ChunkedMessageOperation,
                                                            subArray));
            }
            return result;
        }

        protected abstract byte[] DoConcreteSend(object proxy, byte[] toSend);
        protected abstract byte[] DoConcreteSend(object proxy, List<byte[]> chunkBytes);

        #region IDisposable

        private bool _disposed;

        protected ServiceClientBase(ICacheProvider cacheProvider, ITransportSettings settings,
                                    List<ServerInfo> serverInfos)
        {
            if (cacheProvider == null) throw new ArgumentNullException("cacheProvider");
            if (settings == null) throw new ArgumentNullException("settings");
            if (serverInfos == null) throw new ArgumentNullException("serverInfos");

            if (serverInfos.Count(x => x.ServerId == Guid.Empty) > 0)
            {
                throw new ArgumentException("The serverId cannot be an empty value");
            }


            CacheProvider = cacheProvider;
            Settings = settings;
            ServerInfos = serverInfos;


            GetNextAvailableServer();
        }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        


        protected void GetNextAvailableServer()
        {
            if (_server == null)
            {
                _server = ServerInfos.SingleOrDefault(x => x.IsLocal);
                if (_server == null)
                {
                    _server = ServerInfos.FirstOrDefault();
                }
            }
        }


        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    var realClient = GetClient<TClient>();
                    if (realClient != null)
                    {
                        CacheProvider.Remove(CacheKey);
                        var c = realClient as IDisposable;

                        if (c != null)
                            c.Dispose();
                        realClient = null;
                    }
                }

                _disposed = true;
            }
        }

        #endregion
    }
}