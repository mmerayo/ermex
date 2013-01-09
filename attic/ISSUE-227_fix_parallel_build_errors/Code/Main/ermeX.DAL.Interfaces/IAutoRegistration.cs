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

namespace ermeX.DAL.Interfaces
{
    /// <summary>
    /// Registers items that dont need to be exhanged. The system items
    /// </summary>
    internal interface IAutoRegistration
    {
        bool CreateRemoteComponentInitialSetOfData(Guid remoteComponentId, string ip, int port);
        //void RegisterRemoteSystemServices(Type typeService, Guid remoteComponentId);

        //void RegisterRemoteSystemService(Guid componentId, string serviceImplementationMethodName,Type serviceInterfaceType);

        /// <summary>
        ///   creates the initial set of data
        /// </summary>
        void CreateLocalSetOfData(int port);
    }
}
