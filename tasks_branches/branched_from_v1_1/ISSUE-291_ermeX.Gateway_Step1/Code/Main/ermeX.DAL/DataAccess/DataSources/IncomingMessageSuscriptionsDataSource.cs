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
using System.Diagnostics;
using NHibernate;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    //TODO:refactor to base

    internal sealed class IncomingMessageSuscriptionsDataSource : DataSource<IncomingMessageSuscription>,
                                                           IIncomingMessageSuscriptionsDataSource, IDataAccessUsable<IncomingMessageSuscription>
    {
        private IDataAccessUsable<OutgoingMessageSuscription> OutgoingMessageSuscriptionsDataSource { get; set; }

        [Inject]
        public IncomingMessageSuscriptionsDataSource(IDalSettings dataAccessSettings,
                                                     IComponentSettings componentSettings,
            IOutgoingMessageSuscriptionsDataSource outgoingMessageSuscriptionsDataSource, IDataAccessExecutor dataAccessExecutor)
            : this(dataAccessSettings, componentSettings.ComponentId,dataAccessExecutor)
        {
            if (outgoingMessageSuscriptionsDataSource == null)
                throw new ArgumentNullException("outgoingMessageSuscriptionsDataSource");
            OutgoingMessageSuscriptionsDataSource = (IDataAccessUsable<OutgoingMessageSuscription>)outgoingMessageSuscriptionsDataSource;
        }

        public IncomingMessageSuscriptionsDataSource(IDalSettings dataAccessSettings, Guid componentId, IDataAccessExecutor dataAccessExecutor)
            : base(dataAccessSettings, componentId,dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return IncomingMessageSuscription.TableName; }
        }

        #region IIncomingMessageSuscriptionsDataSource Members

        public IList<IncomingMessageSuscription> GetByMessageType(string bizMessageType)
        {
            return base.GetItemsByField("BizMessageFullTypeName", bizMessageType);
        }

        public IncomingMessageSuscription GetByHandlerId(Guid suscriptionHandlerId)
        {
            if (suscriptionHandlerId.IsEmpty())
                throw new ArgumentException();

            return base.GetItemByField("SuscriptionHandlerId", suscriptionHandlerId);
        }

        public void RemoveByHandlerId(Guid suscriptionId)
        {
            RemoveByProperty("SuscriptionHandlerId", suscriptionId.ToString());
        }

        public IncomingMessageSuscription GetByHandlerAndMessageType(Type handlerType, Type messageType)
        {
            if (handlerType == null) throw new ArgumentNullException("handlerType");
            if (messageType == null) throw new ArgumentNullException("messageType");
            return
                base.GetItemByFields(new[]
                                         {
                                             new Tuple<string, object>("HandlerType", handlerType.FullName),
                                             new Tuple<string, object>("BizMessageFullTypeName", messageType.FullName)
                                         });
        }


        public void SaveIncommingSubscription(Guid suscriptionHandlerId, Type handlerType, Type messageType)
        {
            var result = DataAccessExecutor.Perform(session =>
                {
                    var incomingMessageSuscription = new IncomingMessageSuscription
                    {
                        ComponentOwner = LocalComponentId,
                        BizMessageFullTypeName = messageType.FullName,
                        DateLastUpdateUtc = DateTime.UtcNow,
                        SuscriptionHandlerId = suscriptionHandlerId,
                        HandlerType = handlerType.FullName
                    };
                    Save(session, incomingMessageSuscription);

                    //only one outgoing subscription per message and component,self subscription
                    int countItems =
                        OutgoingMessageSuscriptionsDataSource.CountItems(session, new[]{
                            new Tuple<string, object>("BizMessageFullTypeName", messageType.FullName),
                            new Tuple<string, object>("Component", LocalComponentId),
                            new Tuple<string, object>("ComponentOwner", LocalComponentId)});
                    if (countItems == 0)
                    {
                        var outgoingMessageSuscription = new OutgoingMessageSuscription(incomingMessageSuscription,
                                                                                        LocalComponentId,
                                                                                        LocalComponentId);
                        OutgoingMessageSuscriptionsDataSource.Save(session, outgoingMessageSuscription);
                    }
                    return new DataAccessOperationResult<bool>() {Success = true};
                });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetAll");

        }

        #endregion
    }
}