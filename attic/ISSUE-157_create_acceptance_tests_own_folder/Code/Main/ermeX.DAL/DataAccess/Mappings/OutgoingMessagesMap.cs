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
    internal abstract class OutgoingMessagesMap : ClassMap<OutgoingMessage>
    {
        protected abstract DbEngineType EngineType { get; }

        protected OutgoingMessagesMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, OutgoingMessage.FinalTableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", OutgoingMessage.FinalTableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(OutgoingMessage.GetDbFieldName("Id"));
            Map(x => x.PublishedBy).Column(OutgoingMessage.GetDbFieldName("PublishedBy"));
            Map(x => x.PublishedTo).Column(OutgoingMessage.GetDbFieldName("PublishedTo"));
            Map(x => x.BusMessageId).Column(OutgoingMessage.GetDbFieldName("BusMessageId"));
            Map(x => x.TimePublishedUtc).Column(OutgoingMessage.GetDbFieldName("TimePublishedUtc")).CustomType(
                typeof (DateTimeUserType));
            Map(x => x.Tries).Column(OutgoingMessage.GetDbFieldName("Tries"));
          
            Map(x => x.Failed).Column(OutgoingMessage.GetDbFieldName("Failed"));
            Map(x => x.ComponentOwner).Column(OutgoingMessage.GetDbFieldName("ComponentOwner"));
            Map(x => x.Version).Column(OutgoingMessage.GetDbFieldName("Version"));
            Map(x => x.Delivering).Column(OutgoingMessage.GetDbFieldName("Delivering"));
        }
    }
}