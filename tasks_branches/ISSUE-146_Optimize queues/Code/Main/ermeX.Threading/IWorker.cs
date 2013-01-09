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
using System.Threading;

namespace ermeX.Threading
{
    internal interface IWorker:IDisposable
    {
        /// <summary>
        /// Indicates the worker that there are pending tasks to be done
        /// </summary>
        AutoResetEvent WorkPendingEvent { get; }

        /// <summary>
        /// Indicates the client that there pending tasks have been done
        /// </summary>
        AutoResetEvent FinishedWorkPendingEvent { get; }

        /// <summary>
        /// Indicates the Client that the workers has finished and can be disposed if needed
        /// </summary>
        AutoResetEvent FinishedEvent { get; }

        /// <summary>
        /// Forces the worker to finish
        /// </summary>
        void Exit();

        /// <summary>
        /// Thats it but, please dont use
        /// </summary>
        void Kill();
        /// <summary>
        /// Starts the work
        /// </summary>
        /// <param name="data"></param>
        void StartWorking(object data);

        /// <summary>
        /// indicates whether it finished with error or not
        /// </summary>
        bool ErrorResult { get; }

        string WorkerName { get; set; }
        EventHandler ThreadFinished { get; set; }
        EventHandler PendingWorkFinished { get; set; }
    }
}