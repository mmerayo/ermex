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
using System.Configuration;
using Ninject.Modules;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Bus.Listening;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Bus.Publishing;
using ermeX.Bus.Publishing.ClientProxies;
using ermeX.Bus.Publishing.Dispatching;
using ermeX.Bus.Publishing.Dispatching.Messages;
using ermeX.Bus.Publishing.Dispatching.ServiceRequests;
using ermeX.Bus.Synchronisation;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;

namespace ermeX.Bus.IoC
{
    internal class BusInjections : NinjectModule
    {
        private readonly IBusSettings _settings;

        public BusInjections(IBusSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            List<string> errors;
            if (!new BusSettingsValidator(settings).Validate(out errors))
            {
                throw new ConfigurationException(string.Join(",", errors));
            }
            _settings = settings;
        }

        public override void Load()
        {
            Bind<IEsbManager>().To<EsbManager>().InSingletonScope();
            //TODO: CHANGE THIS when configuration has values passing the whole section
            Bind<IBusSettings>().ToConstant(_settings);

            //TODO:
            Bind<IMessageListener>().To<MessageListeningManager>().InSingletonScope();
            Bind<IListeningManager>().To<ListeningManager>().InSingletonScope();
            //if (_settings.Publisher != null)
            //    Bind<IMessagePublisher>().ToConstant(_settings.Publisher);
            //else
                Bind<IMessagePublisher>().To<MessagePublishingManager>().InSingletonScope();
                Bind<IListeningStrategyProvider>().To<ListeningStrategyProvider>().InSingletonScope();

            Bind<IServiceRequestManager>().To<ServiceRequestManager>().InSingletonScope();
            Bind<IServiceRequestDispatcher>().To<ServiceRequestDispatcher>().InSingletonScope();
            Bind<IServiceCallsProxy>().To<ServiceCallsProxy>();

            Bind<IMessagePublisherDispatcherStrategy>().To<MessageCollector>().InSingletonScope();
            Bind<IMessageDistributor>().To<MessageDistributor>().InSingletonScope();
            Bind<IMessageSubscribersDispatcher>().To<MessageSubscribersDispatcher>().InSingletonScope();

            Bind<IScheduler>().To<IncommingMessagesFifoScheduler>().InSingletonScope();

            Bind<IQueueDispatcherManager>().To<QueueDispatcherManager>().InSingletonScope();
            Bind<IReceptionMessageDistributor>().To<ReceptionMessageDistributor>().InSingletonScope();
        }
    }
}