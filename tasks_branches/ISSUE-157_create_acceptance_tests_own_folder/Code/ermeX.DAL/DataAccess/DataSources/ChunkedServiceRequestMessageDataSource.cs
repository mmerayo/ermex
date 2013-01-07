// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.DataSources
{
    internal class ChunkedServiceRequestMessageDataSource : DataSource<ChunkedServiceRequestMessageData>, IChunkedServiceRequestMessageDataSource, IDataAccessUsable<ChunkedServiceRequestMessageData>
    {
        [Inject]
        public ChunkedServiceRequestMessageDataSource(IDalSettings settings, IComponentSettings componentSettings, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentSettings.ComponentId, dataAccessExecutor)
        {
        }
        public ChunkedServiceRequestMessageDataSource(IDalSettings settings, Guid componentOwner, IDataAccessExecutor dataAccessExecutor)
            : base(settings, componentOwner, dataAccessExecutor)
        {
        }

        protected override string TableName
        {
            get { return ChunkedServiceRequestMessageData.TableName; }
        }

        public ChunkedServiceRequestMessageData GetByCorrelationIdAndOrder(Guid correlationId, int order)
        {
            return GetItemByFields(new[]
                {new Tuple<string, object>("CorrelationId", correlationId), new Tuple<string, object>("Order", order)});
        }
    }
}