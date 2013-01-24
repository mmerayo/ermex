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
using System.IO;
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;
using ermeX.Exceptions;

using ermeX.LayerMessages;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Entities;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Receiving.Server;
using ermeX.Transport.Reception.ServicesHandling;

namespace ermeX.Transport.Reception
{

    internal abstract class ServerBase : IServer
    {
        private const string ChunkMessagesFolderName = "Chunks";
        public static Guid ChunkedMessageOperation = new Guid("5A429BD6-ED3A-426F-9352-D3CB9585A447");
        private readonly IDictionary<Guid, IServiceHandler> _handlers = new ConcurrentDictionary<Guid, IServiceHandler>();
        protected readonly object SyncLock=new object();

        protected ServerBase(ServerInfo serverInfo, IServiceDetailsDataSource dataSourceServices,IChunkedServiceRequestMessageDataSource chunkedServiceRequestMessageDataSource)
        {
            if (serverInfo == null) throw new ArgumentNullException("serverInfo");
            if (dataSourceServices == null) throw new ArgumentNullException("dataSourceServices");

            if (!Directory.Exists(ChunksFolder))
            {
                Directory.CreateDirectory(PathUtils.GetApplicationFolderPath(ChunkMessagesFolderName));
            }

            ServerInfo = serverInfo;
            DataSourceServices = dataSourceServices;
            ChunkedServiceRequestMessageDataSource = chunkedServiceRequestMessageDataSource;
            if (Networking.PortIsBusy((ushort) serverInfo.Port))
                throw new ermeXTcpException(String.Format("The given port #{0} is busy ", serverInfo.Port));
        }

        public ServerInfo ServerInfo { get; set; }
        internal IServiceDetailsDataSource DataSourceServices { get; set; }
        private IChunkedServiceRequestMessageDataSource ChunkedServiceRequestMessageDataSource { get; set; }
        protected readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);

        private static string ChunksFolder
        {
            get { return PathUtils.GetApplicationFolderPath(ChunkMessagesFolderName); }
        }

        #region IDisposable

        private bool _disposed;

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                foreach (var handler in _handlers.Values)
                {
                    handler.Dispose();
                }

                _disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion

        #region IServer Members

        public abstract void StartListening();

        public void RegisterHandler(Guid serviceOperation, IServiceHandler handler)
        {
            if (!_handlers.ContainsKey(serviceOperation))
                lock (SyncLock)
                    if (!_handlers.ContainsKey(serviceOperation))
                        _handlers.Add(serviceOperation, handler);
        }

        #endregion

        protected ServiceResult DoHandleRequestTask(ServiceRequestMessage message)
        {
            ServiceResult result = null;
            try
            {
                result = HandleBizMessage(message);

            }
            catch (Exception ex)
            {
                Logger.Warn(x=>x("DoHandleRequestTask", ex));
                result = new ServiceResult(false);
                result.ServerMessages.Add(ex.ToString());
            }
            return result;
        }

        protected ServiceResult DoHandleChunk(ChunkedServiceRequestMessage message)
        {
            try
            {
                if (message.Operation == ChunkedMessageOperation)
                {
                    var composedMsg = HandleChunk(message);
                    if (composedMsg != null)
                        HandleBizMessage(composedMsg);
                }
                return new ServiceResult(true);
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "DoHandleChunk.{0}", ex));
                throw ex;
            }
        }

        private ServiceRequestMessage HandleChunk(ChunkedServiceRequestMessage message)
        {

            if (message.CorrelationId == Guid.Empty)
                throw new InvalidOperationException("The correlation Id cannot be empty for chunked sequences");

            ServiceRequestMessage result = null;

            if (!message.Eof)
            {
                ChunkedServiceRequestMessageDataSource.Save(message);
            }
            else
            {
                byte[] source;
                {
                    var byteList = new List<byte>();
                    for (int i = 0; i < message.Order; i++)
                    {

                        ChunkedServiceRequestMessage chunk =ChunkedServiceRequestMessage.FromData(
                            ChunkedServiceRequestMessageDataSource.GetByCorrelationIdAndOrder(message.CorrelationId, i));

                        var realBytes = chunk.Data;

                        byteList.AddRange(realBytes);
                        ChunkedServiceRequestMessageDataSource.Remove(chunk);
                    }
                    byteList.AddRange(message.Data);

                    source = byteList.ToArray();
                }
                result = ObjectSerializer.DeserializeObject<ServiceRequestMessage>(source);
            }

            return result;

        }

        private ServiceResult HandleBizMessage(ServiceRequestMessage message)
        {
            ServiceResult result;
            if (EnsureServiceHandlerIsLoaded(message.ServerId, message.Operation))
            {
                var serviceHandler = _handlers[message.Operation];

                object handlerResult;
                if (message.Operation == OperationIdentifiers.InternalMessagesOperationIdentifier)
                {
                    Logger.Trace(x=>x("{0} - Message received ", message.Data.MessageId));
                    handlerResult = serviceHandler.Handle(message.Data);
                }
                else
                {
                    handlerResult = serviceHandler.Handle(message);
                }
                result = new ServiceResult(true)
                             {
                                 ResultData = handlerResult,
                                 AsyncResponseId = message.CallingContextId
                             };
                Logger.Debug(x=>x("handlerResult: {0}", handlerResult) );

            }
            else
            {
                result = new ServiceResult(false);
                string errTxt = string.Format("The requested service with id:{0} is not handled by the current component", message.Operation);
                result.ServerMessages.Add(
                    errTxt);
                Logger.Warn(x=>x("{0}",errTxt));

            }
            return result;
        }

        private bool EnsureServiceHandlerIsLoaded(Guid serverId, Guid operationId)
        {
            if (_handlers.ContainsKey(operationId))
                return true;

            var current = DataSourceServices.GetByOperationId(serverId, operationId);
            if (current == null)
                return false;

            //ensures one only instance per type
            IServiceHandler obj;
            if (_handlers.Values.SingleOrDefault(x => x.GetType().FullName == current.ServiceImplementationTypeName) !=
                null)
                obj =
                    _handlers.Values.SingleOrDefault(x => x.GetType().FullName == current.ServiceImplementationTypeName);
            else
            {
                var serviceType = TypesHelper.GetTypeFromDomain(current.ServiceInterfaceTypeName,true,false);
                if (!IoCManager.Kernel.GetBindings(serviceType).Any())
                    //they are all binded by the MesaggeListeningManager
                {
                    var instance = ObjectBuilder.FromTypeName<IService>(current.ServiceImplementationTypeName);
                    IoCManager.Kernel.Bind(serviceType).ToConstant(instance);
                }

                var realHandlerInstance = (IService) IoCManager.Kernel.Get(serviceType);

                obj = new ServiceRequestDispacher(realHandlerInstance, DataSourceServices);//TODO: INJECTable and injected
            }

            RegisterHandler(operationId, obj);
            return true;
        }
    }
}