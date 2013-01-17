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
using System.Threading;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers;
using ermeX.Common;

using ermeX.Threading;

namespace ermeX.Tests.Services.Mock
{
    internal class MockMessagesProcessorWorker : Worker, IIncomingMessagesProcessorWorker
    {
        public MockMessagesProcessorWorker()
            : base( "MockMessagesDispatcherWorker")
        {
        }

        public bool Started { get; private set; }

        public bool NewItemArrivedFlagged { get; private set; }

        public void Work(object events)
        {
            var syncEvents = ((SyncEvents[]) events)[0];

           

            int index;
            while ((index = WaitHandle.WaitAny((syncEvents).EventArray)) != SyncEvents.EXIT_INDEX)
            {
                if (index != SyncEvents.NewItemArrived)
                    continue;

               
            }
        }

        public override void StartWorking(object data)
        {
            Started = true;
           base.StartWorking(data);
        }

        protected override void DoWork(object data)
        {
            NewItemArrivedFlagged = true;
        }
    }
}