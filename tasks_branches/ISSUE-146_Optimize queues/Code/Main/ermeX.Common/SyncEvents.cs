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

namespace ermeX.Common
{
    public sealed class SyncEvents
    {
        public const int NewItemArrived = 0;

        public const int EXIT_INDEX = 2;

        public const int NewDispatchableItem = 1;
        private readonly WaitHandle[] _eventArray;
        private readonly EventWaitHandle _exitThreadEvent;
        private readonly EventWaitHandle _newArrivedItemEvent;
        private readonly EventWaitHandle _newDispatchableItemEvent;

        public SyncEvents()
        {
            _newArrivedItemEvent = new AutoResetEvent(false);
            _newDispatchableItemEvent = new AutoResetEvent(false);
            _exitThreadEvent = new ManualResetEvent(false);
            _eventArray = new WaitHandle[3];
            _eventArray[NewItemArrived] = _newArrivedItemEvent;
            _eventArray[EXIT_INDEX] = _exitThreadEvent;
            _eventArray[NewDispatchableItem] = NewDispatchableItemEvent;
        }

        public EventWaitHandle ExitThreadEvent
        {
            get { return _exitThreadEvent; }
        }

        public EventWaitHandle NewItemArrivedEvent
        {
            get { return _newArrivedItemEvent; }
        }

        public WaitHandle[] EventArray
        {
            get { return _eventArray; }
        }

        public EventWaitHandle NewDispatchableItemEvent
        {
            get { return _newDispatchableItemEvent; }
        }
    }
}