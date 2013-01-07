// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using Ninject;
using Remotion.Linq.Utilities;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.ConfigurationManagement.Settings.Data;
using ermeX.DAL.DataAccess.Providers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    //TODO:refactor to base
    internal class OutgoingMessageSuscriptionsDataSource : DataSource<OutgoingMessageSuscription>,
                                                           IOutgoingMessageSuscriptionsDataSource,
        IDataAccessUsable<OutgoingMessageSuscription>
    {
        [Inject]
        public OutgoingMessageSuscriptionsDataSource(IDalSettings dataAccessSettings,
                                                     IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : this(dataAccessSettings, componentSettings.ComponentId,dataAccessExecutor)
        {
        }

        public OutgoingMessageSuscriptionsDataSource(IDalSettings dataAccessSettings, Guid componentId, IDataAccessExecutor dataAccessExecutor)
            : base(dataAccessSettings, componentId,dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return OutgoingMessageSuscription.TableName; }
        }

        #region IOutgoingMessageSuscriptionsDataSource Members

        

        public void SaveFromOtherComponent(IncomingMessageSuscription request)
        {
            var result = DataAccessExecutor.Perform(session =>
                {
                    int countItems =
                       CountItems(session, new[]{
                            new Tuple<string, object>("BizMessageFullTypeName", request.BizMessageFullTypeName),
                            new Tuple<string, object>("Component", request.ComponentOwner),
                            new Tuple<string, object>("ComponentOwner", LocalComponentId)});
                    if (countItems == 0)
                    {
                        //TODO: refactor TO outgoing message subscriptions controller
                        var subscriptionToSave = new OutgoingMessageSuscription(request, request.ComponentOwner,
                                                                                LocalComponentId);
                        SaveFromOtherComponent(session,subscriptionToSave,
                                               new[]
                                                   {
                                                       new Tuple<string, object>("Component", request.ComponentOwner),
                                                       new Tuple<string, object>("BizMessageFullTypeName",
                                                                                 request.BizMessageFullTypeName)
                                                   });
                    }
                    return new DataAccessOperationResult<bool>() {Success = true};
                });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation SaveFromOtherComponent");

        }

       

        #endregion

        protected override bool BeforeUpdating(OutgoingMessageSuscription entity, ISession session)
        {
            var existing = GetByMessageType(session, entity.BizMessageFullTypeName, entity.Component);
            session.Evict(existing);
            if (existing == null)
                return true;

            if (existing.Version > entity.Version)
                return false;
            return true;
        }


        //protected internal override bool HasRowAlready(OutgoingMessageSuscription entity, ISession session)
        //{
        //    return CountItems(
        //               new Tuple<string, object>("BizMessageFullTypeName", entity.BizMessageFullTypeName),
        //               new Tuple<string, object>("Component", entity.Component),
        //               new Tuple<string, object>("ComponentOwner", entity.ComponentOwner))>0;
        //}

        public IList<OutgoingMessageSuscription> GetByMessageType(string bizMessageType)
        {
            if (string.IsNullOrEmpty(bizMessageType))
                throw new ArgumentNullException("bizMessageType");
            return base.GetItemsByField("BizMessageFullTypeName", bizMessageType);
        }

        public IList<OutgoingMessageSuscription> GetByMessageType(ISession session, string bizMessageType)
        {
            if (string.IsNullOrEmpty(bizMessageType))
                throw new ArgumentNullException("bizMessageType");
            return base.GetItemsByField(session,"BizMessageFullTypeName", bizMessageType);
        }

        private OutgoingMessageSuscription GetByMessageType(string bizMessageFullTypeName, Guid component)
        {
            if (component == Guid.Empty)
                throw new ArgumentEmptyException("component");
            return GetByMessageType(bizMessageFullTypeName).SingleOrDefault(x => x.Component == component && x.ComponentOwner==LocalComponentId);
        }

        private OutgoingMessageSuscription GetByMessageType(ISession session, string bizMessageFullTypeName, Guid component)
        {
            if (component == Guid.Empty)
                throw new ArgumentEmptyException("component");
            return GetByMessageType(session, bizMessageFullTypeName).SingleOrDefault(x => x.Component == component && x.ComponentOwner == LocalComponentId);
        }
       
    }
}