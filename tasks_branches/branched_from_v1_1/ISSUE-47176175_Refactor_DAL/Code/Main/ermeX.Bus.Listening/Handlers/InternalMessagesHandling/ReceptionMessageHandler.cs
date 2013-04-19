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
using System.IO;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Queues;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;
using ermeX.Threading.Scheduling;
using ermeX.Transport.Interfaces;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling
{
	/// <summary>
	/// Receives a transport message and initiates the incommingworkflow
	/// </summary>
	internal sealed class ReceptionMessageHandler : MessageHandlerBase<TransportMessage>
	{
		private readonly IReadIncommingQueue _queueReader;
		private readonly IWriteIncommingQueue _queueWritter;
		public static Guid OperationIdentifier = OperationIdentifiers.InternalMessagesOperationIdentifier;

		#region IDisposable

		//TODO: REMOVE IF NOT NEEDED
		private bool _disposed;

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{

				}
				_disposed = true;
			}
		}

		public override void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ReceptionMessageHandler()
		{
			Dispose(false);
		}


		#endregion


		[Inject]
		public ReceptionMessageHandler(IReadIncommingQueue queueReader,
			IWriteIncommingQueue queueWritter,
			IReceptionMessageDistributor receptionMessageDistributor,
			IBusSettings settings, IQueueDispatcherManager queueDispatcherManager)
		{
			_queueReader = queueReader;
			_queueWritter = queueWritter;
			if (receptionMessageDistributor == null) throw new ArgumentNullException("receptionMessageDistributor");
			if (settings == null) throw new ArgumentNullException("settings");
			if (queueDispatcherManager == null) throw new ArgumentNullException("queueDispatcherManager");
			ReceptionMessageDistributor = receptionMessageDistributor;
			Settings = settings;
			QueueDispatcherManager = queueDispatcherManager;

			SystemTaskQueue.Instance.EnqueueItem(EnqueueNonDistributedMessages); //reenqueues non dispatched messages on startup
		}

		private IReceptionMessageDistributor ReceptionMessageDistributor { get; set; }
		private IBusSettings Settings { get; set; }
		private IQueueDispatcherManager QueueDispatcherManager { get; set; }
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ReceptionMessageHandler).FullName);

		public override object Handle(TransportMessage message)
		{
			Logger.DebugFormat("Handle",message);
			BusMessage busMessage = message.Data;

			var incomingMessage = new IncomingMessage(BusMessage.Clone(busMessage))
				{
					ComponentOwner = Settings.ComponentId,

					PublishedTo = Settings.ComponentId,
					TimeReceivedUtc = DateTime.UtcNow,
					SuscriptionHandlerId = Guid.Empty,
					Status = Message.MessageStatus.ReceiverReceived,
				};


			//this must be done on-line in case of errors so it returns an exception to the caller
			_queueWritter.Save(incomingMessage);
			ReceptionMessageDistributor.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage));
			Logger.Trace(x => x("{0} - Message received ", message.Data.MessageId));
			return null; //Check the correctness of this null
		}


		/// <summary>
		/// Messages that havent finished the distribution. the distributor ensures that wont distribute it twice to the same suscriber
		/// </summary>
		private void EnqueueNonDistributedMessages()
		{
			//Gets all that were not distributed in previous sessions
			var incomingMessages = _queueReader.GetNonDistributedMessages();//TODO: ISSUE-281: THIS METHOD IS NOT COMMON IN A QUEUE

			foreach (var incomingMessage in incomingMessages)
				ReceptionMessageDistributor.EnqueueItem(new ReceptionMessageDistributor.MessageDistributorMessage(incomingMessage));
		}

		public void RegisterSuscriber(Action<Guid, object> onMessageReceived)
			//TODO: THIS IS A bootch and it must be refactored
		{
			if (onMessageReceived == null) throw new ArgumentNullException("onMessageReceived");
			Logger.Trace("InternalMessageHandler.RegisterSuscriber");

			QueueDispatcherManager.DispatchMessage += onMessageReceived;
		}
	}
}