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
using ermeX;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.Models.Entities;


namespace ermeX.Bus.Synchronisation.Dialogs.HandledByService
{
    [ServiceContract("06A43D33-79EC-428B-8327-82471BF8AEAF", true)]
    internal interface IPublishedServicesDefinitionsService : IService
    {
        /// <summary>
        ///   get all the services in the servers component
        /// </summary>
        /// <param name="request"> </param>
        /// <returns> </returns>
        [ServiceOperation("8551201E-F2C3-4C7C-8473-3372F97BD6C6")]
        PublishedServicesResponseMessage RequestDefinitions(PublishedServicesRequestMessage request);

        /// <summary>
        ///   Adds the services to the component
        /// </summary>
        /// <param name="service"> </param>
        [ServiceOperation("4485F944-4590-48A5-B69C-B072B62D3A0A")]
        void AddService(ServiceDetails service);

        /// <summary>
        ///   Adds the services to the component
        /// </summary>
        /// <param name="services"> </param>
        [ServiceOperation("095D0266-C707-4770-9F1E-F4BE60921A04")]
        void AddServices(IList<ServiceDetails> services);
    }
}