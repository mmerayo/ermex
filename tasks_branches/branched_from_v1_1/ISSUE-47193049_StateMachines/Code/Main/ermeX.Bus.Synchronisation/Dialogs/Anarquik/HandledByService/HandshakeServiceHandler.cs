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
using System.Diagnostics;
using System.Net;
using System.Threading;

using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByService;
using ermeX.Bus.Synchronisation.Messages;
using ermeX.ComponentServices.Interfaces;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Connectivity;
using ermeX.Exceptions;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
	internal sealed class HandshakeServiceHandler : IHandshakeService
	{
		private readonly ICanWriteComponents _componentsWritter;
		private readonly IRegisterComponents _componentsRegistrator;
		private readonly ICanReadConnectivityDetails _connectivityReader;
		private readonly IComponentManager _componentManager;


		[Inject]
		public HandshakeServiceHandler(IMessagePublisher publisher, IMessageListener listener,
		                               IComponentSettings settings,
		                               ICanReadComponents componentReader,
		                               ICanWriteComponents componentsWritter,
		                               IComponentManager componentManager, IRegisterComponents componentsRegistrator,
		                               ICanReadConnectivityDetails connectivityReader)
		{
			Logger = LogManager.GetLogger(typeof (HandshakeServiceHandler), settings.ComponentId, LogComponent.Handshake);
			_componentsWritter = componentsWritter;
			_componentsRegistrator = componentsRegistrator;
			_connectivityReader = connectivityReader;
			if (publisher == null) throw new ArgumentNullException("publisher");
			if (listener == null) throw new ArgumentNullException("listener");
			if (settings == null) throw new ArgumentNullException("settings");
			if (componentManager == null) throw new ArgumentNullException("componentManager");
			Publisher = publisher;
			Listener = listener;
			Settings = settings;
			ComponentReader = componentReader;

			_componentManager = componentManager;
		}


		private IMessagePublisher Publisher { get; set; }

		private IMessageListener Listener { get; set; }
		private IComponentSettings Settings { get; set; }
		private ICanReadComponents ComponentReader { get; set; }

		private readonly ILogger Logger;

		//this handler id is static

		#region IHandshakeService Members

		public MyComponentsResponseMessage RequestJoinNetwork(JoinRequestMessage message)
		{
			//if (!_componentManager.LocalComponent.IsRunning())
			//    throw new ermeXComponentNotStartedException(Settings.ComponentId);

			if(Settings.ComponentId==message.SourceComponentId)
				throw new InvalidOperationException("Cannot request join to itself");

			try
			{
				Logger.Trace(x => x("RequestJoinNetwork RECEIVED on {0} from {1}", Settings.ComponentId,
				                    message.SourceComponentId));

				_componentManager.AddRemoteComponent(message.SourceComponentId, IPAddress.Parse(message.SourceIp),
													 (ushort)message.SourcePort, false);

				//TODO: THIS WAS REMOVED _componentsRegistrator.CreateRemoteComponent(message.SourceComponentId, message.SourceIp, message.SourcePort);

				//prepare result
				var componentsDatas = new List<Tuple<AppComponent, ConnectivityDetails>>();

				var components = new List<AppComponent>(ComponentReader.FetchAll());

				foreach (var appComponent in components)
				{
					var connectivityDetails = _connectivityReader.Fetch(appComponent.ComponentId);
					Debug.Assert(connectivityDetails != null, "connectivity details cannot be null");
					var tuple = new Tuple<AppComponent, ConnectivityDetails>(appComponent,
					                                                         connectivityDetails);
					componentsDatas.Add(tuple);
				}
				var result = new MyComponentsResponseMessage(message.SourceComponentId, componentsDatas);


				Logger.Trace(x => x("RequestJoinNetwork HANDLED on {0} from {1}", Settings.ComponentId,
				                    message.SourceComponentId));

				return result;
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("RequestJoinNetwork: Error while handling request. Exception: {0}", ex));
				throw ex;
			}
		}

		public void HandshakeCompleted(Guid componentId)
		{
			try
			{
				Logger.DebugFormat("HandshakeCompleted - ComponentId:{0}", componentId);
				var remoteComponent = _componentManager.GetRemoteComponent(componentId);
				remoteComponent.JoinedRemotely();
			}
			catch (Exception ex)
			{
				Logger.Error(x => x("HandshakeCompleted: Error while handling request. Exception: {0}", ex));
				throw ex;
			}
		}

		#endregion
	}
}