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

namespace ermeX.Biz.Interfaces
{
    internal interface IServicesManager
    {
        TServiceInterface GetServiceProxy<TServiceInterface>() where TServiceInterface : IService;

        /// <summary>
        ///   When there are several components publishing the same sevice, i.e. system services it specifies concretely which component to get the proxy for
        /// </summary>
        /// <typeparam name="TServiceInterface"> </typeparam>
        /// <param name="componentId"> </param>
        /// <returns> </returns>
        TServiceInterface GetServiceProxy<TServiceInterface>(Guid componentId) where TServiceInterface : IService;
    }
}