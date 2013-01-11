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
using System.Diagnostics;
using ermeX.Common;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Receiving.Server;

namespace ermeX.Transport.Reception
{
    internal class MainServerHandler : MarshalByRefObject, IServerHandlerContract
    {
        #region IServerHandlerContract Members

        public byte[] Execute(byte[] input)
        {
            ServiceResult result = null;
            try
            {


                var message = ObjectSerializer.DeserializeObject<ServiceRequestMessage>(input);
                if (message.Operation == ServerBase.ChunkedMessageOperation)
                {
                    var deserializeObject =
                        ObjectSerializer.DeserializeObject<ChunkedServiceRequestMessage>(input);
                    result = OnChunkReceived(deserializeObject);
                }
                else
                {
                    result = OnRequestReceived(message);
                }
            }
            catch (Exception ex)
            {
                Debugger.Break(); //TODO LOG EXCEPTION SOMETHING HAPPEND WHILE PROCESSING REQUEST
                throw;
            }
            byte[] ret = ObjectSerializer.SerializeObjectToByteArray(result);

            return ret;
        }

        #endregion

        public event ReceptionDelegates.RequestHandler RequestReceived;
        public event ReceptionDelegates.ChunkRequestHandler ChunkReceived;

        public ServiceResult OnChunkReceived(ChunkedServiceRequestMessage message)
        {
            var handler = ChunkReceived;
            if (handler != null)
                return handler(message);
            return null;
        }

        public ServiceResult OnRequestReceived(ServiceRequestMessage request)
        {
            var handler = RequestReceived;
            if (handler != null)
                return handler(request);

            return null;
        }
    }
}