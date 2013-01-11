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
using System.Data;
using System.Linq;
using NHibernate;
using Ninject;
using ermeX.ConfigurationManagement.Settings;

using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    //TODO:REFACTOR ALL THE DATAACCESS LAYER
    //TODO:refactor to base
    internal class IncomingMessagesDataSource : DataSource<IncomingMessage>, IIncomingMessagesDataSource, IDataAccessUsable<IncomingMessage>
    {

        [Inject]
        public IncomingMessagesDataSource(IDalSettings dataAccessSettings, IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : this( dataAccessSettings, componentSettings.ComponentId,dataAccessExecutor)
        {
        }

        public IncomingMessagesDataSource(IDalSettings dataAccessSettings, Guid componentId, IDataAccessExecutor dataAccessExecutor)
            : base(dataAccessSettings, componentId,dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return IncomingMessage.FinalTableName; }
        }

        protected override bool BeforeUpdating(IncomingMessage entity, ISession session)
        {
            if (entity.BusMessage == null || entity.Status == BusMessageData.BusMessageStatus.NotSet)
                throw new InvalidOperationException("Must set the bus message or the status");
            return base.BeforeUpdating(entity, session);

        }

        public IncomingMessage GetNextDispatchableItem(int maxLatency)
        {
            var res = DataAccessExecutor.Perform(session => GetNextDispatchableItem(session,maxLatency));
            if (!res.Success)
                throw new DataException("Couldnt perform the operation GetNextDispatchableItem");

            return res.ResultValue;
        }

        private DataAccessOperationResult<IncomingMessage> GetNextDispatchableItem( ISession session,int maxLatency)
        {
            //TODO: IMPROVE

            IncomingMessage result = null;
            var incomingMessages = GetAll(session, new Tuple<string, bool>("TimePublishedUtc", true)).ResultValue;

            //goes through all to find the first dispatchable
            foreach (var incomingMessage in incomingMessages)
            {
                var timeSpan = DateTime.UtcNow.Subtract(incomingMessage.TimePublishedUtc);
                var milliseconds = timeSpan.TotalMilliseconds;
                if (milliseconds >= maxLatency)
                {

                    if (incomingMessage.Status == BusMessageData.BusMessageStatus.ReceiverDispatchable)
                    {
                        incomingMessage.Status = BusMessageData.BusMessageStatus.ReceiverDispatching;
                        Save(session, incomingMessage);
                    }
                    else
                    {
                        continue;
                    }
                    result = incomingMessage;
                    break;
                }
            }
            return new DataAccessOperationResult<IncomingMessage>(true, result);
        }

        public IEnumerable<IncomingMessage> GetMessagesToDispatch()
        {
            //TODO: improve
            return
                GetAll().Where(x => x.Status == BusMessageData.BusMessageStatus.ReceiverReceived).OrderBy(
                    x => x.TimePublishedUtc).ToList();

        }
    }
}