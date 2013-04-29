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
using ermeX.Models.Entities;
using ermeX.LayerMessages;

namespace ermeX.Bus.Interfaces.Dispatching
{
    /// <summary>
    ///   Interface for a dispatcher strategy
    /// </summary>
    internal interface IMessagePublisherDispatcherStrategy : IDisposable
    {
        /// <summary>
        ///   Status of the dispatcher
        /// </summary>
        DispatcherStatus Status { get; }

        /// <summary>
        ///   Dispatches one message publishing it
        /// </summary>
        /// <param name="messageToPublish"> </param>
        void Dispatch(BusMessage messageToPublish);

        /// <summary>
        ///   It starts the strategy
        /// </summary>
        void Start();

        /// <summary>
        ///   It stops the strategy
        /// </summary>
        void Stop();
    }
}