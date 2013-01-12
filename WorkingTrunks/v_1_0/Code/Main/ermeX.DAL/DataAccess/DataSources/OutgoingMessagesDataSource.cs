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
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    //TODO:REFACTOR ALL THE DATAACCESS LAYER
    //TODO:refactor to base
    internal class OutgoingMessagesDataSource : DataSource<OutgoingMessage>, IOutgoingMessagesDataSource, IDataAccessUsable<OutgoingMessage>
    {
        [Inject]
        public OutgoingMessagesDataSource(IDalSettings dataAccessSettings, IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : this(dataAccessSettings, componentSettings.ComponentId,dataAccessExecutor)
        {
        }

        public OutgoingMessagesDataSource(IDalSettings dataAccessSettings, Guid componentId, IDataAccessExecutor dataAccessExecutor)
            : base(dataAccessSettings, componentId,dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return "OutgoingMessages"; }
        }

        protected override bool BeforeUpdating(OutgoingMessage entity, NHibernate.ISession session)
        {
            if(entity.Status==Message.MessageStatus.NotSet)
                throw new InvalidOperationException("Must set the status");
            return base.BeforeUpdating(entity, session);

        }

        #region IOutgoingMessagesDataSource Members

        public IEnumerable<OutgoingMessage> GetItemsPendingSorted()
        {
            return GetAll(new Tuple<string, bool>("Tries", true), new Tuple<string, bool>("CreatedTimeUtc", true)).
                Where(
                    x => x.Failed == false); //TODO: OPTIMISE FROM QUERY
        }

        public OutgoingMessage GetNextDeliverable()
        {
            var outgoingMessages =
                GetItemsByFields(new[]
                                     {
                                         new Tuple<string, object>("Delivering", false),
                                         new Tuple<string, object>("Failed", false)
                                     });
            if (outgoingMessages.Count == 0)
                return null;

            var oldestPublishingTime = outgoingMessages.Min(x => x.CreatedTimeUtc.Ticks);
            return
                outgoingMessages.OrderBy(x => x.Id).FirstOrDefault(x => x.CreatedTimeUtc.Ticks == oldestPublishingTime);
        }

        #endregion
    }
}