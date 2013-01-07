// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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