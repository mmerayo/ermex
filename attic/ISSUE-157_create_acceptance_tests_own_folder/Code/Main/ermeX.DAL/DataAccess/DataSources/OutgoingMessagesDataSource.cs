// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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

        #region IOutgoingMessagesDataSource Members

        public IEnumerable<OutgoingMessage> GetItemsPendingSorted()
        {
            return GetAll(new Tuple<string, bool>("Tries", true), new Tuple<string, bool>("TimePublishedUtc", true)).
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

            var oldestPublishingTime = outgoingMessages.Min(x => x.TimePublishedUtc.Ticks);
            return
                outgoingMessages.OrderBy(x => x.Id).FirstOrDefault(x => x.TimePublishedUtc.Ticks == oldestPublishingTime);
        }

        #endregion
    }
}