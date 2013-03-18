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
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces
{
    internal interface IServiceDetailsDataSource : IDataSource<ServiceDetails>
    {
        /// <summary>
        ///   Gets the local server service details
        /// </summary>
        /// <param name="operationId"> </param>
        /// <returns> </returns>
        ServiceDetails GetByOperationId(Guid operationId);

        /// <summary>
        ///   Gets any server service details
        /// </summary>
        /// <param name="operationId"> </param>
        /// <returns> </returns>
        ServiceDetails GetByOperationId(Guid publisher, Guid operationId);

        IList<ServiceDetails> GetByInterfaceType(Type interfaceType);
        IList<ServiceDetails> GetByInterfaceType(string interfaceTypeFullName);
        IList<ServiceDetails> GetByMethodName(string interfaceTypeName, string methodName);
        ServiceDetails GetByMethodName(string interfaceTypeName, string methodName, Guid publisherComponent);
        IList<ServiceDetails> GetLocalCustomServices();
    }
}