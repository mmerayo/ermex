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
using System.Configuration;
using System.Linq;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using ermeX.Common;
using ermeX.Tests.Services.Mock;
using ermeX.Transport.BuiltIn.SuperSocket.Server;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Receiving.Server;

namespace ermeX.Tests.Services.Builtin.SuperSockets
{
    internal class DummyTestSuperSocketServer<TMessage> : MockTestServerBase<TMessage>
    {
        /// <summary>
        /// defines a result for an operation
        /// </summary>
       

        private readonly int _port;
        private readonly List<DummySocketServerResult> _dummyResults;

        public DummyTestSuperSocketServer(int port):this(port,null)
        {
        }

        public DummyTestSuperSocketServer(int port, AutoResetEvent eventDone):this(port,eventDone,null)
        {
        }

        public DummyTestSuperSocketServer(int port, AutoResetEvent eventDone, List<DummySocketServerResult> dummyResults)
        {
            if (dummyResults!=null && dummyResults.Select(x => x.OperationId).Any(operationId => dummyResults.Count(x=>x.OperationId==operationId)>1))
            {
                throw new ArgumentException("Only one response per operationId is supported");
            }

            _port = port;
            _dummyResults = dummyResults;
            if(eventDone!=null)
                Init(eventDone);
        }

        private ServerImpl RealServer { get; set; }

        public List<DummySocketServerResult> DummyResults
        {
            get { return _dummyResults; }
        }

        public void Init(AutoResetEvent eventDone)
        {
            //TODO: CONFIGURABLE
            var rootConfig = new RootConfig
                                 {
                                     MaxWorkingThreads = 1024,
                                     MaxCompletionPortThreads = 500,
                                     MinWorkingThreads = 15,
                                     MinCompletionPortThreads = 15
                                 };

#if DEBUG
            rootConfig.LoggingMode = LoggingMode.Console;
#endif
            var serverConfig = new ServerConfig
                                   {
                                       Ip = "Any",
                                       LogCommand = true,
                                       MaxConnectionNumber = 1024,
                                       Mode = SocketMode.Sync,
                                       Name = "Async Socket Server",
                                       Port = _port,
                                       ClearIdleSession = true,
                                       ClearIdleSessionInterval = 1,
                                       IdleSessionTimeOut = 1,
                                       MaxCommandLength = 20*1024*1024 //25MB
                                   };

            RealServer = new ServerImpl(ReceivedMessages, eventDone);
            if(DummyResults!=null)
                RealServer.SetResult(DummyResults);
            RealServer.Setup(rootConfig, serverConfig, SocketServerFactory.Instance);
            if (!RealServer.IsRunning)
                RealServer.Start();
        }

     
        public override void Dispose()
        {
            if (RealServer != null)
                RealServer.Dispose();
            base.Dispose();
        }

        #region Nested type: ServerImpl

        private class ServerImpl : AppServer<ExposedSession, BinaryCommandInfo>, IServerHandlerProvider,
                                   IServerHandlerContract
        {
            private readonly AutoResetEvent _eventDone;
            private readonly List<TMessage> _receivedMessages;
            private List<DummySocketServerResult> _results = null;

            public ServerImpl(List<TMessage> receivedMessages, AutoResetEvent eventDone)
                : base(new DataReceptionProtocol())
            {
                if (receivedMessages == null) throw new ArgumentNullException("receivedMessages");
                _receivedMessages = receivedMessages;
                _eventDone = eventDone;
            }

            public IServerHandlerContract ServerHandler
            {
                get { return this; }
            }

            public byte[] Execute(byte[] input)
            {
                var obj = ObjectSerializer.DeserializeObject<TMessage>(input);
                _receivedMessages.Add(obj);

                var serviceResult = new ServiceResult(true);
                
                serviceResult.ServerMessages.Add("AA");
                if (_results != null)
                {
                    var serviceRequestMessage =  obj as ServiceRequestMessage;
                    if(serviceRequestMessage!=null)
                    {
                        try
                        {
                            serviceResult.ResultData =
                                _results.Single(x => x.OperationId == serviceRequestMessage.Operation).ResultForResponse;                            
                        }catch(Exception ex)
                        {
                            throw new ConfigurationException("You need to configure the response of this service",ex);
                        }
                    }
                }

                if ((_eventDone != null && !(obj is ChunkedServiceRequestMessage))
                    ||
                    (_eventDone != null && obj is ChunkedServiceRequestMessage &&
                     (obj as ChunkedServiceRequestMessage).Eof))
                    _eventDone.Set();

                if (serviceResult.ResultData == typeof(void))
                    return null;
                
                return ObjectSerializer.SerializeObjectToByteArray<ServiceResult>(serviceResult);
            }

            public void SetResult(List<DummySocketServerResult> dummyResults)
            {
                _results = dummyResults;
            }
        }

        #endregion
    }
    public class DummySocketServerResult
    {
        //for void returns
        public DummySocketServerResult(Guid operationId):this(operationId,typeof(void))
        {
            
        }

        public DummySocketServerResult(Guid operationId,object resultForResponse)
        {
            if(operationId==Guid.Empty) throw new ArgumentException("operationId");

            OperationId = operationId;
            ResultForResponse = resultForResponse;
        }

        public Guid OperationId { get; set; }

        public object ResultForResponse { get; set; }

    }
}