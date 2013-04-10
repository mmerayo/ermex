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
using System.Text;
using System.Threading;
using Common.Logging;
using Ninject;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Queues;
using ermeX.Entities.Entities;
using ermeX.LayerMessages;
using ermeX.Threading.Queues;
using ermeX.Threading.Scheduling;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    //THIS CLASS IS CRAP WE NEED TO IMPROVE
    //TODO: IT MUST BE IN PARALLEL, THIS IS TEMPORAL UNTIL iSSUE-244 is developed
    //TODO: THE WHOLE CLAS LOGIC SHOULD BE REWRITTEN
    internal sealed class QueueDispatcherManager : ProducerSequentialConsumerPriorityQueue<QueueDispatcherManager.QueueDispatcherManagerMessage>, IQueueDispatcherManager
    {
        public class QueueDispatcherManagerMessage
        {
            public IncomingMessage IncomingMessage { get; private set; }
            public bool MustCalculateLatency { get; set; }

            public QueueDispatcherManagerMessage(IncomingMessage message, bool mustCalculateLatency=true)
            {

                if (message == null) throw new ArgumentNullException("message");
                IncomingMessage = message;
                MustCalculateLatency = mustCalculateLatency;
            }
        }

        private class QueueComparer:IComparer<QueueDispatcherManagerMessage>
        {
            public int Compare(QueueDispatcherManagerMessage x, QueueDispatcherManagerMessage y)
            {
                if (x == null && y == null)
                    return 0;
                if (x == null)
                    return -1;
                if (y == null)
                    return 1;

                var xi = x.IncomingMessage;
                var yi = y.IncomingMessage;
                 if (xi == null && yi == null)
                    return 0;
                if (xi == null)
                    return -1;
                if (yi == null)
                    return 1;

                return xi.CreatedTimeUtc.CompareTo(yi.CreatedTimeUtc);
            }
        }

        [Inject]
        public QueueDispatcherManager(IBusSettings settings,
			IWriteIncommingQueue incommingQueueWriter,
			ICanReadLatency latenciesReader,
			ICanUpdateLatency latenciesUpdater)
			:base(new QueueComparer())
        {
            if (settings == null) throw new ArgumentNullException("settings");
            Settings = settings;
	        IncommingQueueWriter = incommingQueueWriter;
	        LatenciesReader = latenciesReader;
	        LatenciesUpdater = latenciesUpdater;
        }
        private IBusSettings Settings { get; set; }
	    private IWriteIncommingQueue IncommingQueueWriter { get; set; }
	    private ICanReadLatency LatenciesReader { get; set; }
	    private ICanUpdateLatency LatenciesUpdater { get; set; }

	    protected override Func<QueueDispatcherManagerMessage,bool> RunActionOnDequeue
        {
            get { return DoDeliver; }
        }

        private bool DoDeliver(QueueDispatcherManagerMessage message)
        {
            bool result;
            DateTime receivedHere = DateTime.UtcNow;

            IncomingMessage incomingMessage = message.IncomingMessage;

            if (message.MustCalculateLatency)
            {
                UpdateComponentLatency(incomingMessage.ToBusMessage(), receivedHere);
                message.MustCalculateLatency = false; //this will ensure that its only calculated once
            }

            if (MustWaitDueToQueueLatency(receivedHere, incomingMessage))
            {
                result = false;
            }
            else
            {
                incomingMessage.Status = Message.MessageStatus.ReceiverDispatching;
                IncommingQueueWriter.Save(incomingMessage);

                Logger.Trace(x => x("{0} Start Handling", incomingMessage.MessageId));
                OnDispatchMessage(incomingMessage.SuscriptionHandlerId, incomingMessage.ToBusMessage());

				IncommingQueueWriter.Remove(incomingMessage);
                Logger.Trace(x => x("{0} Handled finally", incomingMessage.MessageId));
                result= true;
            }
            return result;
        }

        /// <summary>
        ///  checks if it must wait because the latency barrier wasnt reached.
        /// </summary>
        /// <param name="receivedHere"></param>
        /// <param name="incomingMessage"></param>
        /// <returns></returns>
        /// <remarks>It does the wait</remarks>
        private bool MustWaitDueToQueueLatency(DateTime receivedHere, IncomingMessage incomingMessage)
        {
            bool result;
            int maxLatency = LatenciesReader.GetMaxLatency();
                //get the latency of all the components involved in the queue

            //we equal the latency
            TimeSpan timeSpan = receivedHere.Subtract(incomingMessage.CreatedTimeUtc);
            var millisecondsLatency = (int) timeSpan.TotalMilliseconds;
            if (millisecondsLatency < maxLatency) //checks the components latency IF IT WAS DELIVERED TO SOON
            {
                //lets wait the expected latency
                Thread.Sleep(maxLatency - millisecondsLatency);
                result = true; //if will reenqueue it, and as is sorted it will go to the beggining
            }
            else
            {
                result = false;
            }
            return result;
        }

        private void UpdateComponentLatency(BusMessage receivedMessage, DateTime receivedTimeUtc)
        {
            var milliseconds = receivedTimeUtc.Subtract(receivedMessage.CreatedTimeUtc).Milliseconds;
            if (milliseconds <= (Settings.MaxDelayDueToLatencySeconds * 1000))
            {
                LatenciesUpdater.RegisterComponentRequestLatency(receivedMessage.Publisher, milliseconds);
            }
        }

        public event Action<Guid, object> DispatchMessage;

        //TODO: invocation must be done on the biz layer
        private void OnDispatchMessage(Guid suscriptionHandlerId, BusMessage message)
        {
            try
            {
                Action<Guid, object> handler = DispatchMessage;
                if (handler != null)
                {
                    handler(suscriptionHandlerId, message.Data.RawData);
                    Logger.Trace(x=>x("Receiver: Dispatched message with id: {0}", message.MessageId));
                }
                else
                {
                    Logger.Trace(x=>x("Receiver: The message with id: {0} didnt have receivers configured", message.MessageId));
                }

            }catch(Exception ex)
            {
                Logger.Error(x=>x("Error handling message {0} by subscriptionHandler {1}", message.MessageId, suscriptionHandlerId),ex);
                throw;
            }
        }
    }
}
