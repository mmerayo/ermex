// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
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
    internal class ConnectivityDetailsDataSource : DataSource<ConnectivityDetails>, IConnectivityDetailsDataSource, IDataAccessUsableConnectivityDetails
    {
        [Inject]
        public ConnectivityDetailsDataSource(IDalSettings dataAccessSettings,
                                             IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : this(dataAccessSettings, componentSettings.ComponentId,dataAccessExecutor)
        {
        }

        public ConnectivityDetailsDataSource(IDalSettings dataAccessSettings, Guid componentId,IDataAccessExecutor dataAccessExecutor)
            : base(dataAccessSettings, componentId,dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return ConnectivityDetails.TableName; }
        }

        #region IConnectivityDetailsDataSource Members

        public ConnectivityDetails GetByComponentId(Guid componentId)
        {
            //The priority is the order
            return GetItemByField("ServerId", componentId); //TODO: OPTIMIZE THIS. 
        }

        public ConnectivityDetails GetByComponentId(ISession session,Guid componentId)
        {
            //The priority is the order
            return GetItemByField(session,"ServerId", componentId); //TODO: OPTIMIZE THIS. 
        }

        #endregion

        protected override bool BeforeUpdating(ConnectivityDetails entity, ISession session)
        {
            if (entity.ServerId.IsEmpty())
                throw new ArgumentException("Entity has not a valid property value, serverId");

            var existing = GetByComponentId(session,entity.ServerId);
            session.Evict(existing);
            if (existing == null)
                return true;

            if (existing.Version > entity.Version)
                return false;
            return true;
        }

       
        protected override void UpdateWhenExternal(ConnectivityDetails entity, ConnectivityDetails existingEntity)
        {
            base.UpdateWhenExternal(entity, existingEntity);
            entity.IsLocal = existingEntity.IsLocal;
        }
    }
}