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

namespace ermeX.LayerMessages
{
    internal interface ISystemMessage
    {
        /// <summary>
        /// The message unique identifier
        /// </summary>
        /// <remarks>This must be unique accross the whole system</remarks>
        Guid MessageId { get; }

        DateTime CreatedTimeUtc { get; }
        
    }

    /// <summary>
    /// Represents a system message
    /// </summary>
    internal interface ISystemMessage<TMessage> : ISystemMessage
    {
        /// <summary>
        /// The layer inner data
        /// </summary>
        TMessage Data { get; }
    }
}