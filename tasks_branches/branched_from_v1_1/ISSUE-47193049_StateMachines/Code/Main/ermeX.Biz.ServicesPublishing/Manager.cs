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
using System.Linq;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Services;


namespace ermeX.Biz.ServicesPublishing
{
    internal class Manager : IServicePublishingManager
    {
	    private readonly ICanReadComponents _componentReader;
	    private readonly ICanReadServiceDetails _serviceDetailsReader;
	    private readonly IComponentSettings _componentSettings;

	    [Inject]
        public Manager(IMessagePublisher publisher, 
			IMessageListener listener, 
			ICanReadComponents componentReader,
			ICanReadServiceDetails serviceDetailsReader,
			IComponentSettings componentSettings)
        {
	       
		    if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            Publisher = publisher;
            Listener = listener;
			_componentReader = componentReader;
			_serviceDetailsReader = serviceDetailsReader;
		    _componentSettings = componentSettings;
        }

        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }

        #region IServicePublishingManager Members

		public void PublishService<TServiceInterface>() where TServiceInterface : IService
		{
			//TODO: CONSIDER REMOVAL OF THIS
			Listener.PublishService<TServiceInterface>();
		}

        public void PublishService<TServiceInterface>(Type serviceImplementationType) where TServiceInterface : IService
        {
	        PublishService(typeof (TServiceInterface), serviceImplementationType);
        }

        public void PublishService(Type serviceInterface, Type serviceImplementation)
        {
            //TODO: THIS IS REDUNDANT
            Listener.PublishService(serviceInterface, serviceImplementation);


			if (serviceInterface == null) throw new ArgumentNullException("serviceInterface");
			if (serviceImplementation == null) throw new ArgumentNullException("serviceImplementation");
			var appComponents = _componentReader.FetchOtherComponents();

			//ISSUE-281: From reader
			var serviceOperations =
				_serviceDetailsReader.GetByInterfaceType(serviceInterface.FullName).Where(x => x.Publisher == _componentSettings.ComponentId).
					ToList();

			foreach (var appComponent in appComponents)
			{
				var proxy = Publisher.GetServiceProxy<IPublishedServicesDefinitionsService>(appComponent.ComponentId);
				foreach (var serviceOperation in serviceOperations)
				{
					proxy.AddService(serviceOperation);
				}
			}

        }

        #endregion
    }
}