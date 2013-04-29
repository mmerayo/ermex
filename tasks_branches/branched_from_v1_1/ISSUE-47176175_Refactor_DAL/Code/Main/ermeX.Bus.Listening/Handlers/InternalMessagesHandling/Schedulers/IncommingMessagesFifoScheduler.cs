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
using ermeX.DAL.Interfaces;
using ermeX.DAL.Interfaces.Component;
using ermeX.DAL.Interfaces.Queues;
using ermeX.Models.Entities;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers
{
    internal class IncommingMessagesFifoScheduler : IScheduler
    {
        [Inject]
		public IncommingMessagesFifoScheduler(IReadIncommingQueue incomingQueue, 
                                              ICanReadLatency latencyReader)
        {
	        IncomingQueue = incomingQueue;
	        LatencyReader = latencyReader;
        }

	    public IReadIncommingQueue IncomingQueue { get; set; }
	    public ICanReadLatency LatencyReader { get; set; }

	    #region IScheduler Members

        public IncomingMessage GetNext()
        {
            var maxLatency = LatencyReader.GetMaxLatency();
			return IncomingQueue.GetNextDispatchableItem(maxLatency);
        }

        

        #endregion
    }
}