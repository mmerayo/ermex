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
using ermeX.Interfaces;

namespace ermeX.Bus.Interfaces
{
    internal interface IDialogsManager
    {
        ///// <summary>
        /////   Request configured node(Anarquik) - nodes - service providers - message suscriptions
        ///// </summary>
        ///// <param name="componentId"> </param>
        //void JoinNetworkComponent(Guid componentId);

        void JoinNetwork();

        /// <summary>
        ///   Comunicates the subscription to every currently registered component
        /// </summary>
        /// <param name="subscriptionHandlerId"> </param>
        /// <param name="notifyComponents"> true notifies other components </param>
        void Suscribe(Guid subscriptionHandlerId);

        void NotifyService<TService>(Type serviceImplementationType) where TService : IService;
        void NotifyService(Type serviceInterface, Type serviceImplementation);
        void ExchangeDefinitions();
        void ExchangeDefinitions(AppComponent appComponent);

        void NotifyCurrentStatus();
        void UpdateRemoteServiceDefinition(string interfaceName, string methodName);
        void UpdateRemoteServiceDefinition(string interfaceName, string methodName, AppComponent appComponent);
        void EnsureDefinitionsAreExchanged(AppComponent appComponent, int retries=1);
        void EnsureDefinitionsAreExchanged(IEnumerable<AppComponent> appComponents,int retries=1);
    }
}