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
using ermeX.Entities.Entities;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers
{
    internal class IncommingMessagesFifoScheduler : IScheduler
    {
        [Inject]
        public IncommingMessagesFifoScheduler(IIncomingMessagesDataSource dataSource, 
                                              IAppComponentDataSource componentsDataSource)
        {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (componentsDataSource == null) throw new ArgumentNullException("componentsDataSource");
            DataSource = dataSource;
            ComponentsDataSource = componentsDataSource;
        }

        private IIncomingMessagesDataSource DataSource { get; set; }
        private IAppComponentDataSource ComponentsDataSource { get; set; }
        
        #region IScheduler Members

        public IncomingMessage GetNext()
        {
            var maxLatency = ComponentsDataSource.GetMaxLatency();
            return DataSource.GetNextDispatchableItem(maxLatency);
        }

        

        #endregion
    }
}