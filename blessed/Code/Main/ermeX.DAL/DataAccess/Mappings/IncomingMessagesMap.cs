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
    internal abstract class IncomingMessagesMap : ClassMap<IncomingMessage>
    {
        protected abstract DbEngineType EngineType { get; }

        protected IncomingMessagesMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, IncomingMessage.FinalTableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", IncomingMessage.FinalTableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(IncomingMessage.GetDbFieldName("Id"));
            Map(x => x.PublishedBy).Column(IncomingMessage.GetDbFieldName("PublishedBy"));
            Map(x => x.PublishedTo).Column(IncomingMessage.GetDbFieldName("PublishedTo"));
            Map(x => x.SuscriptionHandlerId).Column(IncomingMessage.GetDbFieldName("SuscriptionHandlerId"));
            Map(x => x.BusMessageId).Column(IncomingMessage.GetDbFieldName("BusMessageId"));
            Map(x => x.TimePublishedUtc).Column(IncomingMessage.GetDbFieldName("TimePublishedUtc")).CustomType(
                typeof (DateTimeUserType));
            Map(x => x.TimeReceivedUtc).Column(IncomingMessage.GetDbFieldName("TimeReceivedUtc")).CustomType(
                typeof (DateTimeUserType));
            Map(x => x.ComponentOwner).Column(IncomingMessage.GetDbFieldName("ComponentOwner"));
            Map(x => x.Version).Column(IncomingMessage.GetDbFieldName("Version"));
        }
    }
}