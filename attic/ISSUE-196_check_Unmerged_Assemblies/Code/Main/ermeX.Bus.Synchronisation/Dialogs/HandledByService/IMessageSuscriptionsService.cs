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
using System.Collections.Generic;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.Entities.Entities;
using ermeX.Interfaces;

namespace ermeX.Bus.Synchronisation.Dialogs.HandledByService
{
    [ServiceContract("D3DF02B7-4C62-4048-A322-633FF548ADAC", true)]
    internal interface IMessageSuscriptionsService : IService
    {
        /// <summary>
        ///   Requests all the subscriptions from a component
        /// </summary>
        /// <param name="request"> </param>
        /// <returns> The incoming and outgoing suscriptions from the service side </returns>
        [ServiceOperation("5E211520-B5D7-4B74-A4AD-569E09559B0D")]
        MessageSuscriptionsResponseMessage RequestSuscriptions(MessageSuscriptionsRequestMessage request);

        /// <summary>
        ///   Requests to add a subscription
        /// </summary>
        /// <param name="request"> </param>
        [ServiceOperation("75FABA0D-5318-4FDB-B418-1F475DFC2BA7")]
        void AddSuscription(IncomingMessageSuscription request);

        /// <summary>
        ///   Requests to add subscriptions
        /// </summary>
        /// <param name="request"> </param>
        [ServiceOperation("32211B80-73A8-4775-8A08-A7533E273CFB")]
        void AddSuscriptions(IList<IncomingMessageSuscription> request);
    }
}