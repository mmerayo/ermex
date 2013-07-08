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
using ermeX.DAL.Interfaces.Subscriptions;
using ermeX.Exceptions;
using ermeX.Logging;
using ermeX.Models.Entities;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByService
{
	internal sealed class MessageSuscriptionsRequestMessageHandler : IMessageSuscriptionsService
	{
		private readonly ICanReadIncommingMessagesSubscriptions _incommingSubscriptionsReader;
		private readonly ICanReadOutgoingMessagesSubscriptions _outgoingSubscriptionsReader;
		private readonly ICanUpdateOutgoingMessagesSubscriptions _outgoingSubscriptionsWritter;
		private readonly IComponentManager _componentManager;

		[Inject]
		public MessageSuscriptionsRequestMessageHandler(IMessagePublisher publisher,
		                                                IMessageListener listener,
		                                                ICanReadIncommingMessagesSubscriptions incommingSubscriptionsReader,
		                                                ICanReadOutgoingMessagesSubscriptions outgoingSubscriptionsReader,
		                                                ICanUpdateOutgoingMessagesSubscriptions outgoingSubscriptionsWritter,
		                                                IComponentSettings settings,
			IComponentManager componentManager)
		{
			Logger = LogManager.GetLogger(typeof (MessageSuscriptionsRequestMessageHandler), settings.ComponentId,
			                              LogComponent.Handshake);
			_incommingSubscriptionsReader = incommingSubscriptionsReader;
			_outgoingSubscriptionsReader = outgoingSubscriptionsReader;
			_outgoingSubscriptionsWritter = outgoingSubscriptionsWritter;
			_componentManager = componentManager;
			Publisher = publisher;
			Listener = listener;
			Settings = settings;
		}

		private IMessagePublisher Publisher { get; set; }

		private IMessageListener Listener { get; set; }
		private IComponentSettings Settings { get; set; }
		private readonly ILogger Logger;

		#region IMessageSuscriptionsService Members

		public MessageSuscriptionsResponseMessage RequestSuscriptions(MessageSuscriptionsRequestMessage request)
		{
			var result = new MessageSuscriptionsResponseMessage(Settings.ComponentId, request.CorrelationId)
				{
					MyIncomingSuscriptions = _incommingSubscriptionsReader.FetchAll(),
					MyOutgoingSuscriptions =
						_outgoingSubscriptionsReader.FetchAll().Where(
							x => x.Component != request.SourceComponentId).ToList()
				};

			return result;
		}

		public void AddSuscription(IncomingMessageSuscription request)
		{
			Logger.DebugFormat("AddSubscription - HandlerType: {0}, BizMessageFullTypeName={1}", request.HandlerType, request.BizMessageFullTypeName);

			if(!_componentManager.LocalComponent.IsRunning())
				throw new ermeXComponentNotStartedException(Settings.ComponentId);
			
			try
			{
				if (request == null) throw new ArgumentNullException("request");
				_outgoingSubscriptionsWritter.ImportFromOtherComponent(request);
			}
			catch (Exception ex)
			{
				Logger.Warn(x => x("Could not handle request", ex));
			}
		}

		public void AddSuscriptions(IEnumerable<IncomingMessageSuscription> request)
		{
			if (!_componentManager.LocalComponent.IsRunning())
					throw new ermeXComponentNotStartedException(Settings.ComponentId);
			try
			{				
				foreach (var incomingMessageSuscription in request)
				{
					AddSuscription(incomingMessageSuscription);
				}
			}
			catch (Exception ex)
			{
				Logger.Warn(x => x("Could not handle request", ex));
			}
		}

		#endregion
	}
}