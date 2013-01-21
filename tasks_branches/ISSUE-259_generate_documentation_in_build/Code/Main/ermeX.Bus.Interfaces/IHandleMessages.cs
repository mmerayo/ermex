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
namespace ermeX
{
    /// <summary>
    /// Interface of the message handlers
    /// </summary>
    /// <typeparam name="TMessage">type of message to handle</typeparam>
    /// <remarks>implement this itnerface to create a message handler</remarks>
    public interface IHandleMessages<TMessage>
    {
        /// <summary>
        /// Invoked when a message [of TMessage] is published by any component in the ermeX network
        /// </summary>
        /// <param name="message">The type of message to handle<remarks>This can be a base type</remarks></param>
        void HandleMessage(TMessage message);
    }
}