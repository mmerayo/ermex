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
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Services;
using ermeX.LayerMessages;
using ermeX.Transport.Interfaces.Messages;
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Bus.Listening
{
	internal class EsbManager : IEsbManager, IDisposable
	{
		private readonly ICanReadServiceDetails _serviceDetailsReader;

		[Inject]
		public EsbManager(IBusSettings settings, IMessagePublisherDispatcherStrategy messagesDispatcher,
		                  IServiceRequestDispatcher serviceRequestDispatcher,
		                  ICanReadServiceDetails serviceDetailsReader)
		{
			_serviceDetailsReader = serviceDetailsReader;
			if (settings == null) throw new ArgumentNullException("settings");
			if (messagesDispatcher == null) throw new ArgumentNullException("messagesDispatcher");
			if (serviceRequestDispatcher == null) throw new ArgumentNullException("serviceRequestDispatcher");
			Settings = settings;

			MessagesDispatcher = messagesDispatcher;
			ServiceRequestDispatcher = serviceRequestDispatcher;
		}

		private IMessagePublisherDispatcherStrategy MessagesDispatcher { get; set; }

		private IBusSettings Settings { get; set; }

		private IServiceRequestDispatcher ServiceRequestDispatcher { get; set; }

		#region IEsbManager Members

		public void Publish(BusMessage message)
		{
			if (message == null) throw new ArgumentNullException("message");

			//var messageToPublish = GetPublisheableMessage(message);

			MessagesDispatcher.Dispatch(message);
		}

		public void Start()
		{
			lock (this)
			{
				if (MessagesDispatcher.Status == DispatcherStatus.Stopped)
					MessagesDispatcher.Start();
				else
				{
					throw new InvalidOperationException();
				}
			}
		}

		#endregion

		#region Services

		public IServiceOperationResult<TResult> RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
		                                                                object[] requestParams)
		{
			if (destinationComponent.IsEmpty()) throw new ArgumentException();
			ServiceRequestMessage request = GetServiceRequestMessage<TResult>(destinationComponent, serviceOperation,
			                                                                  requestParams);
			return ServiceRequestDispatcher.RequestSync<TResult>(request);
		}


		public void RequestService<TResult>(Guid destinationComponent, Guid serviceOperation,
		                                    Action<IServiceOperationResult<TResult>> responseHandler,
		                                    object[] requestParams)
		{
			if (destinationComponent.IsEmpty()) throw new ArgumentException();
			//TODO: ASYNC FUNCTIONALLITY
			ServiceRequestMessage request = GetServiceRequestMessage<TResult>(destinationComponent, serviceOperation,
			                                                                  requestParams);
			ServiceRequestDispatcher.RequestAsync(request, responseHandler);
		}

		private ServiceRequestMessage GetServiceRequestMessage<TResult>(Guid destinationComponent, Guid operationId,
		                                                                object[] requestParams)
		{
			Guid serverId = destinationComponent != Guid.Empty
				                ? destinationComponent
				                : _serviceDetailsReader.GetByOperationId(operationId).Publisher;

			var callingContextId = typeof (TResult) != typeof (void) ? Guid.NewGuid() : default(Guid);

			//TODO: MUST CHANGE WHEN DEVELOPING CALLER, TO PROVIDE THE CORRECT PARAM NAMES FROM A PROXY COMPILED ON THE FLY, change this interface to provide the parameters and reorganize class etc..... THE NEXT LINEs ONLY COMPILES

			var requestParameters = new Dictionary<string, object>();
			int i = 0;

			foreach (var requestParam in requestParams)
			{
				requestParameters.Add("Param" + i++, requestParam);
			}

			//finished crap TODO
			var result = ServiceRequestMessage.GetForServiceRequest<TResult>(serverId, operationId, callingContextId,
			                                                                 requestParameters);

			return result;
		}

		#endregion

		#region IDisposable

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					MessagesDispatcher.Dispose();
					MessagesDispatcher = null;
				}

				_disposed = true;
			}
		}

		#endregion
	}
}