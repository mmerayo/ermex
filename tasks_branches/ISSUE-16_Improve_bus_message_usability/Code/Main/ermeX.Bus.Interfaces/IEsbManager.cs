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
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Interfaces
{

    //TODO: REMOVE THIS CLASS
    /// <summary>
    ///   Interface for the EsbManager
    /// </summary>
    internal interface IEsbManager
    {
        /// <summary>
        ///   Publishes one message 
        /// </summary>
        /// <param name="message"> </param>
        void Publish(BusMessage message);

        /// <summary>
        ///   Starts the subcomponent
        /// </summary>
        void Start();

        /// <summary>
        ///   Requests a service. It doesnt handle the response
        /// </summary>
        /// <typeparam name="TResult"> </typeparam>
        /// <param name="destinationComponent"> </param>
        /// <param name="serviceOperation"> </param>
        /// <param name="requestParams"> </param>
        /// <returns> </returns>
        IServiceOperationResult<TResult> RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
                                                                 object[] requestParams);

        /// <summary>
        ///   Requests a service. It handles the response
        /// </summary>
        /// <typeparam name="TResult"> </typeparam>
        /// <param name="destinationComponent"> </param>
        /// <param name="serviceOperation"> </param>
        /// <param name="responseHandler"> </param>
        /// <param name="requestParams"> </param>
        void RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
                                     Action<IServiceOperationResult<TResult>> responseHandler, object[] requestParams);
    }
}