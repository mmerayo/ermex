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
using ermeX;
using ermeX.Bus.Synchronisation.Messages;



namespace ermeX.Bus.Synchronisation.Dialogs.HandledByService
{
    [ServiceContract("007DACE3-A4B2-4D79-923A-A5326AB66265", true)]
    internal interface IHandshakeService : IService
    {
        [ServiceOperation("752C3C75-324A-48BB-B55E-56C91F443C72")]
        MyComponentsResponseMessage RequestJoinNetwork(JoinRequestMessage request);

        /// <summary>
        /// Use to communicate the component status
        /// </summary>
        /// <param name="sourceComponentId"></param>
        /// <param name="status"></param>
        /// <returns>The server component status</returns>
        [ServiceOperation("697E533E-491F-4B97-92E7-48C28C40363B")]
        ComponentStatus ExchangeComponentStatus(Guid sourceComponentId, ComponentStatus status);

        /// <summary>
        /// indicates if the definitions can be exchanged and stores who does it
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns></returns>
        [ServiceOperation("35DB8278-BC27-4BF3-AE76-4542B078172E")]
        bool CanExchangeDefinitions(Guid componentId);

        /// <summary>
        /// Releases the definitions exchanger token
        /// </summary>
        /// <param name="componentId"></param>
        [ServiceOperation("084B9665-5422-42CF-8E37-A17415A5F130")]
        void DefinitionsExchanged(Guid componentId);
    }
}