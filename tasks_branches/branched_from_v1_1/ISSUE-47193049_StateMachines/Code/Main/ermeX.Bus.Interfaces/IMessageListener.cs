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
using ermeX.Common;


namespace ermeX.Bus.Interfaces
{
    //TODO: sagas with configurator

	internal interface IMessageListener : IStartable
    {
	    
        /// <summary>
        ///   suscribes to one message
        /// </summary>
        /// <returns> The suscriptionHandlerId </returns>
        Guid Suscribe(Type handlerInterfaceType, object handler);


        /// <summary>
        ///   suscribes to one message
        /// </summary>
        
        /// <param name="handlerInterfaceType"> </param>
        /// <param name="handler"> handler to subscribe </param>
        /// <param name="objHandler"> real subscribed handler </param>
        /// <returns> The suscriptionHandlerId </returns>
        Guid Suscribe(Type handlerInterfaceType, object handler, out object objHandler);
        void PublishService<TServiceInterface>(Type serviceImplementation) where TServiceInterface : IService;

        /// <summary>
        ///   Publishes the IoC injected implementation
        /// </summary>
        /// <typeparam name="TServiceInterface"> </typeparam>
        void PublishService<TServiceInterface>() where TServiceInterface : IService;

        void PublishService(Type serviceInterface, Type serviceImplementation);
    }
}