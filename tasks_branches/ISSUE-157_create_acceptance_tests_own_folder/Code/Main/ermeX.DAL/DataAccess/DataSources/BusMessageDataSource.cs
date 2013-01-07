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
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    internal class BusMessageDataSource : DataSource<BusMessageData>, IBusMessageDataSource, IDataAccessUsable<BusMessageData>
    {
        [Inject]
        public BusMessageDataSource(IDalSettings settings, IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentSettings.ComponentId, dataAccessExecutor)
        {
        }

        public BusMessageDataSource(IDalSettings settings, Guid componentOwner, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentOwner, dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return BusMessageData.TableName; }
        }
        public static string GetTableName()
        {
            return BusMessageData.TableName; 
        }

        public IList<BusMessageData> GetMessagesToDispatch()
        {
           return GetItemsByField("Status", BusMessageData.BusMessageStatus.ReceiverReceived).OrderBy(x=>x.CreatedTimeUtc).ToList();
        }
    }
}