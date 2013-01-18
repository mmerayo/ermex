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
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Interfaces;

namespace ermeX.Biz.ServicesPublishing
{
    internal class Manager : IServicePublishingManager
    {
        [Inject]
        public Manager(IMessagePublisher publisher, IMessageListener listener, IDialogsManager dialogsManager)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (dialogsManager == null) throw new ArgumentNullException("dialogsManager");
            Publisher = publisher;
            Listener = listener;
            DialogsManager = dialogsManager;
        }

        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private IDialogsManager DialogsManager { get; set; }

        #region IServicePublishingManager Members

        public void PublishService<TServiceInterface>(Type serviceImplementationType) where TServiceInterface : IService
        {
            Listener.PublishService<TServiceInterface>(serviceImplementationType);
            DialogsManager.NotifyService<TServiceInterface>(serviceImplementationType);
        }

        public void PublishService<TServiceInterface>() where TServiceInterface : IService
        {
            Listener.PublishService<TServiceInterface>();
        }

        public void PublishService(Type serviceInterface, Type serviceImplementation)
        {
            //TODO: THIS IS REDUNDANT
            Listener.PublishService(serviceInterface, serviceImplementation);
            DialogsManager.NotifyService(serviceInterface,serviceImplementation);

        }

        #endregion
    }
}