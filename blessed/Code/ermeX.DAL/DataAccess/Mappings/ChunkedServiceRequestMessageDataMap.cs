// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using FluentNHibernate.Mapping;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.DAL.DataAccess.Mappings.UserMappingTypes;
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.Mappings
{
    internal abstract class ChunkedServiceRequestMessageDataMap : ClassMap<ChunkedServiceRequestMessageData>
    {
        protected abstract DbEngineType EngineType { get; }

        protected ChunkedServiceRequestMessageDataMap()
        {
            string tableName;
            switch(EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, ChunkedServiceRequestMessageData.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", ChunkedServiceRequestMessageData.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Table( tableName);

            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(ChunkedServiceRequestMessageData.GetDbFieldName("Id"));
            Map(x => x.ComponentOwner).Column(ChunkedServiceRequestMessageData.GetDbFieldName("ComponentOwner"));
            Map(x => x.Version).Column(ChunkedServiceRequestMessageData.GetDbFieldName("Version"));
            Map(x => x.CorrelationId).Column(ChunkedServiceRequestMessageData.GetDbFieldName("CorrelationId"));
            Map(x => x.DataBytes).Column(ChunkedServiceRequestMessageData.GetDbFieldName("Data")).
                CustomType("BinaryBlob").Length(int.MaxValue); 
            Map(x => x.Eof).Column(ChunkedServiceRequestMessageData.GetDbFieldName("Eof"));
            Map(x => x.Operation).Column(ChunkedServiceRequestMessageData.GetDbFieldName("Operation"));
            Map(x => x.Order).Column(ChunkedServiceRequestMessageData.GetDbFieldName("Order"));
            //TODO:restrictions composite key
        }
    }
}