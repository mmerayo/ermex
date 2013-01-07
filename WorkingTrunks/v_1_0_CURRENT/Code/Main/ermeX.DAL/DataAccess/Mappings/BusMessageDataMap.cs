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
    internal abstract class BusMessageDataMap : ClassMap<BusMessageData>
    {
        protected abstract DbEngineType EngineType { get; }

        protected BusMessageDataMap()
        {
            string tableName;
            switch(EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, BusMessageData.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", BusMessageData.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Table( tableName);

            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(BusMessageData.GetDbFieldName("Id"));
            Map(x => x.ComponentOwner).Column(BusMessageData.GetDbFieldName("ComponentOwner"));
            Map(x => x.Version).Column(BusMessageData.GetDbFieldName("Version"));
            Map(x => x.CreatedTimeUtc).Column(BusMessageData.GetDbFieldName("CreatedTimeUtc")).CustomType(typeof (DateTimeUserType));
            Map(x => x.JsonMessage).Column(BusMessageData.GetDbFieldName("JsonMessage"));
            Map(x => x.MessageId).Column(BusMessageData.GetDbFieldName("MessageId"));
            Map(x => x.Publisher).Column(BusMessageData.GetDbFieldName("Publisher"));
            Map(x => x.Status).Column(BusMessageData.GetDbFieldName("Status")).CustomType
                <BusMessageData.BusMessageStatus>();
            //TODO:restrictions composite key
        }
    }
}