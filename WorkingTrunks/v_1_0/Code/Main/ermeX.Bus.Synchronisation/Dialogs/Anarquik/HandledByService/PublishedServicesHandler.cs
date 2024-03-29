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
using System.Linq;
using Common.Logging;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Status;
using ermeX.DAL.Interfaces;

using ermeX.Entities.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
    internal sealed class PublishedServicesHandler : IPublishedServicesDefinitionsService
    {
        [Inject]
        public PublishedServicesHandler(IMessagePublisher publisher, IMessageListener listener,
                                        IServiceDetailsDataSource servicesDataSource, IComponentSettings settings,
            IStatusManager statusManager)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (servicesDataSource == null) throw new ArgumentNullException("servicesDataSource");
            if (settings == null) throw new ArgumentNullException("settings");
            if (statusManager == null) throw new ArgumentNullException("statusManager");
            Publisher = publisher;
            Listener = listener;
            ServicesDataSource = servicesDataSource;
            Settings = settings;
            StatusManager = statusManager;
        }

        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }
        private IServiceDetailsDataSource ServicesDataSource { get; set; }
        private IComponentSettings Settings { get; set; }
        private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);
        private IStatusManager StatusManager { get; set; }

        #region IPublishedServicesDefinitionsService Members

        public PublishedServicesResponseMessage RequestDefinitions(PublishedServicesRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            StatusManager.WaitIsRunning();
            IList<ServiceDetails> localServiceDefinitions;
            if (request.IsSingleResult)
            {
                localServiceDefinitions = new List<ServiceDetails>
                                              {
                                                  ServicesDataSource.GetByMethodName(request.InterfaceName, request.MethodName,Settings.ComponentId)
                                              };
            }
            else
            {
                localServiceDefinitions = ServicesDataSource.GetLocalCustomServices();
                //GetAll().Where(x => x.Publisher == Settings.ComponentId && !x.IsSystemService).ToList();
            }
            var result = new PublishedServicesResponseMessage(Settings.ComponentId, request.CorrelationId)
            {
                LocalServiceDefinitions =localServiceDefinitions
            };
            Logger.Trace(x=>x("RequestDefinitions: Handled server Component:{0} client Component:{1} Results #: {2}",Settings.ComponentId,request.SourceComponentId,result.LocalServiceDefinitions.Count()));
            return result;
        }

        public void AddServices(IList<ServiceDetails> services)
        {
            StatusManager.WaitIsRunning();
            foreach (var serviceDetails in services)
            {
                AddService(serviceDetails);
            }
        }

        public void AddService(ServiceDetails service)
        {
            StatusManager.WaitIsRunning();
            //ServiceDetails existing = ServicesDataSource.GetByOperationId(service.Publisher, service.OperationIdentifier);
            //if (existing != null)
            //    ServicesDataSource.Remove(existing);

            var deterministicFilter = new[]
                                          {
                                              new Tuple<string, object>("OperationIdentifier",
                                                                        service.OperationIdentifier),
                                              new Tuple<string, object>("Publisher", service.Publisher)
                                          };
            
                ServicesDataSource.SaveFromOtherComponent(service,deterministicFilter);
            
            //TODO: TEST THIS as is saving it from the other component
        }

        #endregion
    }
}