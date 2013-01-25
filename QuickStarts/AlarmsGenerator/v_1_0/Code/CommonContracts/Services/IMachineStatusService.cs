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

using CommonContracts.enums;
using ermeX;

namespace CommonContracts.Services
{
    /// <summary>
    /// This is the interface of the service used to explictly request the status of one machine
    /// </summary>
    /// <remarks>As its methods dont retrun values can be exposed by several components</remarks>
    [ServiceContract("8BF21C7F-0181-430A-90F8-747C242632C3")] //Guid taken from: http://www.get-a-guid.com/
    public interface IMachineStatusService : IService
    {
        /// <summary>
        /// Forces publishing the status by the receiver
        /// </summary>
        /// <remarks>Used when the Stock man starts its panel to collect the existing machines and their statuses</remarks>
        [ServiceOperation("08E1C865-A938-4A04-8649-9EE1BE5FDC30")] //Guid taken from: http://www.get-a-guid.com/
        void PublishStatus();

        /// <summary>
        /// Adds items to the stock
        /// </summary>
        /// <remarks>Used by the Stock man panel to add items. 
        /// THIS IS NOT A REAL WORLD SOLUTION, as the client application could invoke all the services
        /// This just illustrates how to specify a single component to attand the request  </remarks>
        [ServiceOperation("F16A532A-5E26-493A-8FAD-7CE2BED2A1C7")] //Guid taken from: http://www.get-a-guid.com/
        void AddItems(DrinkType drink, int numItemsToAdd);
    }
}