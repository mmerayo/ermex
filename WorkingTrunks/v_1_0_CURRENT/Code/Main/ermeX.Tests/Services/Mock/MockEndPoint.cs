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
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.Sending.Client;

namespace ermeX.Tests.Services.Mock
{
    internal class MockEndPoint : IEndPoint
    {
        public readonly List<object> MessagesSent = new List<object>();
        public bool Fails;
        public bool RaisesException;

        public void Dispose()
        {
        }

        public ServiceResult Send(ServiceRequestMessage message)
        {
            if (RaisesException)
                throw new Exception("Failure for testing");
            MessagesSent.Add(message);
            return new ServiceResult(!Fails);
        }
    }
}