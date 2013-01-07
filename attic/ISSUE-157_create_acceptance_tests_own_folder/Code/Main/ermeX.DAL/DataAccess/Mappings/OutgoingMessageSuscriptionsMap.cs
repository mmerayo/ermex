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
    internal abstract class OutgoingMessageSuscriptionsMap : ClassMap<OutgoingMessageSuscription>
    {
        protected abstract DbEngineType EngineType { get; }

        protected OutgoingMessageSuscriptionsMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, OutgoingMessageSuscription.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", OutgoingMessageSuscription.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(OutgoingMessageSuscription.GetDbFieldName("Id"));
            Map(x => x.Component).Column(OutgoingMessageSuscription.GetDbFieldName("ComponentId"));
            Map(x => x.DateLastUpdateUtc).Column(OutgoingMessageSuscription.GetDbFieldName("DateLastUpdateUtc")).
                CustomType(typeof (DateTimeUserType));
            Map(x => x.BizMessageFullTypeName).Column(OutgoingMessageSuscription.GetDbFieldName("BizMessageFullTypeName"))
                .Length(256);
            Map(x => x.ComponentOwner).Column(OutgoingMessageSuscription.GetDbFieldName("ComponentOwner"));
            Map(x => x.Version).Column(OutgoingMessageSuscription.GetDbFieldName("Version"));
            //TODO:restrictions composite key
        }
    }
}