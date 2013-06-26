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

using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ComponentServices.Interfaces;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Services;
using ermeX.Exceptions;
using ermeX.Models.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
	internal sealed class PublishedServicesHandler : IPublishedServicesDefinitionsService
	{
		private readonly ICanReadServiceDetails _serviceDetailsReader;
		private readonly ICanWriteServiceDetails _serviceDetailsWritter;
		private readonly IComponentManager _componentManager;

		[Inject]
		public PublishedServicesHandler(IMessagePublisher publisher,
		                                IMessageListener listener,
		                                ICanReadServiceDetails serviceDetailsReader,
		                                ICanWriteServiceDetails serviceDetailsWritter,
		                                IComponentSettings settings,
		                                IComponentManager componentManager)
		{
			_serviceDetailsReader = serviceDetailsReader;
			_serviceDetailsWritter = serviceDetailsWritter;
			_componentManager = componentManager;
			if (publisher == null) throw new ArgumentNullException("publisher");
			if (listener == null) throw new ArgumentNullException("listener");
			if (settings == null) throw new ArgumentNullException("settings");
			Publisher = publisher;
			Listener = listener;
			Settings = settings;
		}

		private IMessagePublisher Publisher { get; set; }

		private IMessageListener Listener { get; set; }
		private IComponentSettings Settings { get; set; }
		private static readonly ILogger Logger = LogManager.GetLogger(typeof (PublishedServicesHandler).FullName);

		#region IPublishedServicesDefinitionsService Members

		public PublishedServicesResponseMessage RequestDefinitions(PublishedServicesRequestMessage request)
		{
			try
			{
				Logger.DebugFormat("RequestDefinitions - InterfaceName: {0}, MethodName={1}", request.InterfaceName,
				                   request.MethodName);

				if (!_componentManager.LocalComponent.IsRunning())
					throw new ermeXComponentNotStartedException(Settings.ComponentId);
				if (request == null) throw new ArgumentNullException("request");


				IEnumerable<ServiceDetails> localServiceDefinitions;
				if (request.IsSingleResult)
				{
					localServiceDefinitions = new List<ServiceDetails>
						{
							_serviceDetailsReader.GetByMethodName(request.InterfaceName, request.MethodName, Settings.ComponentId)
						};
				}
				else
				{
					localServiceDefinitions = _serviceDetailsReader.GetLocalCustomServices();
				}
				var result = new PublishedServicesResponseMessage(Settings.ComponentId, request.CorrelationId)
					{
						LocalServiceDefinitions = localServiceDefinitions
					};
				Logger.Trace(
					x =>
					x("RequestDefinitions: Handled server Component:{0} client Component:{1} Results #: {2}", Settings.ComponentId,
					  request.SourceComponentId, result.LocalServiceDefinitions.Count()));

				return result;
			}
			catch (Exception ex)
			{
				Logger.Error("RequestDefinitions - Exception: {0}",ex);
				throw;
			}
		}

		public void AddServices(IList<ServiceDetails> services)
		{
			try
			{
				Logger.DebugFormat("AddServices - services.Count: {0}", services.Count);
				if (!_componentManager.LocalComponent.IsRunning())
					throw new ermeXComponentNotStartedException(Settings.ComponentId);
				foreach (var serviceDetails in services)
				{
					AddService(serviceDetails);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("AddService - Exception: {0}", ex);
				throw;
			}
		}

		public void AddService(ServiceDetails service)
		{
			try
			{
				Logger.DebugFormat("AddService - : {1}.{2}::{0}", service.Publisher, service.ServiceInterfaceTypeName,
				                   service.ServiceImplementationMethodName);
				if (!_componentManager.LocalComponent.IsRunning())
					throw new ermeXComponentNotStartedException(Settings.ComponentId);
				_serviceDetailsWritter.ImportFromOtherComponent(service);
			}
			catch (Exception ex)
			{
				Logger.Error("AddService - Exception: {0}", ex);
				throw;
			}
		}

		#endregion
	}
}