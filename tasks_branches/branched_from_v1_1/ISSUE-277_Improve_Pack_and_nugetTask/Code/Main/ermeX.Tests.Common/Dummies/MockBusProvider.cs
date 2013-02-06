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
using ermeX.Bus.Interfaces;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Messages.ServiceOperations;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Tests.Common.Dummies
{
    internal class MockBusProvider : IEsbManager
    {
        

        public void Publish(BusMessage message)
        {
        }

        public void Start()
        {
        }

        public IServiceOperationResult<TResult> RequestService<TResult>(Guid destinationComponent, Guid serviceOperation, object[] requestParams)
        {
            throw new NotImplementedException();
        }

        public void RequestService<TResult>(Guid destinationComponent, Guid serviceOperation, Action<IServiceOperationResult<TResult>> responseHandler, object[] requestParams)
        {
            throw new NotImplementedException();
        }

        public ServiceOperationResult<TResult> RequestService<TResult>(Guid serviceOperation, object[] requestParams)
        {
            throw new NotImplementedException();
        }

        public void RequestService<TResult>(Guid serviceOperation,
                                            Action<IServiceOperationResult<TResult>> responseHandler,
                                            object[] requestParams)
        {
            throw new NotImplementedException();
        }
    }
}