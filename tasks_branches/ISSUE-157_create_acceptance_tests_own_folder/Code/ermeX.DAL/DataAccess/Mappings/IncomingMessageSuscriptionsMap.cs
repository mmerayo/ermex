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
    internal abstract class IncomingMessageSuscriptionsMap : ClassMap<IncomingMessageSuscription>
    {
        protected abstract DbEngineType EngineType { get; }

        protected IncomingMessageSuscriptionsMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, IncomingMessageSuscription.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", IncomingMessageSuscription.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(IncomingMessageSuscription.GetDbFieldName("Id"));
            Map(x => x.DateLastUpdateUtc).Column(IncomingMessageSuscription.GetDbFieldName("DateLastUpdateUtc")).
                CustomType(typeof (DateTimeUserType));
            Map(x => x.BizMessageFullTypeName).Column(IncomingMessageSuscription.GetDbFieldName("BizMessageFullTypeName"))
                .Length(256);
            Map(x => x.ComponentOwner).Column(IncomingMessageSuscription.GetDbFieldName("ComponentOwner"));
            Map(x => x.SuscriptionHandlerId).Column(IncomingMessageSuscription.GetDbFieldName("SuscriptionHandlerId"));
            Map(x => x.Version).Column(IncomingMessageSuscription.GetDbFieldName("Version"));
            Map(x => x.HandlerType).Column(IncomingMessageSuscription.GetDbFieldName("HandlerType"));
            //TODO:restrictions composite key
        }
    }
}