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
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Messages.ServiceOperations;
using ermeX.Transport.Interfaces.Sending.Client;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Tests.Services.Mock
{
    internal class DummyServiceProxy : IServiceProxy
    {
        public TransportMessage LastSentMessage;
        private int calls;
        public int ForceNumTries { get; set; }

        public int Calls
        {
            get { return calls; }
        }

        public void Dispose()
        {
        }


        public ServiceResult Send(TransportMessage message)
        {
            if (ForceNumTries > 0 && (calls = Calls + 1) < ForceNumTries)
            {
                var serviceResult = new ServiceResult(false);
                serviceResult.ServerMessages.Add("Mock failed server message");
                return serviceResult;
            }
            LastSentMessage = message;
            return new ServiceResult(true);
        }

        public ServiceOperationResult<TResult> SendServiceRequestSync<TResult>(IOperationServiceRequestData request)
        {
            throw new NotImplementedException();
        }

        public void SendServiceRequestAsync<TResult>(IOperationServiceRequestData request,
                                                     Action<IServiceOperationResult<TResult>> responseHandler)
        {
            throw new NotImplementedException();
        }

        public ServiceOperationResult<TResult> SendServiceRequestSync<TResult>(ServiceRequestMessage request)
        {
            throw new NotImplementedException();
        }

        public void SendServiceRequestAsync<TResult>(ServiceRequestMessage request,
                                                     Action<IServiceOperationResult<TResult>> responseHandler)
        {
            throw new NotImplementedException();
        }
    }
}