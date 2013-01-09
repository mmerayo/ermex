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

namespace ermeX.Transport.Interfaces.Messages
{
    internal interface IOperationServiceRequestData : IServiceRequestMessage
    {
        Guid ServerId { get; }

        /// <summary>
        ///   The calling context for the operation. allows to trace responses.
        /// </summary>
        /// <remarks>
        ///   Guid.Empty when no reponse is expected
        /// </remarks>
        Guid CallingContextId { get; }

        IDictionary<string, ServiceRequestMessage.RequestParameter> Parameters { get; }
        string ResultType { get; }
    }
}