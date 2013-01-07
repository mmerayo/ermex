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
using ermeX.Entities.Entities;

namespace ermeX.DAL.DataAccess.Mappings
{
    internal abstract class ConnectivityDetailsMap : ClassMap<ConnectivityDetails>
    {
        protected abstract DbEngineType EngineType { get; }

        protected ConnectivityDetailsMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, ConnectivityDetails.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", ConnectivityDetails.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(ConnectivityDetails.GetDbFieldName("Id"));
            Map(x => x.Ip).Column(ConnectivityDetails.GetDbFieldName("Ip")).Unique();
            Map(x => x.Port).Column(ConnectivityDetails.GetDbFieldName("Port"));
            Map(x => x.IsLocal).Column(ConnectivityDetails.GetDbFieldName("IsLocal"));
            Map(x => x.ServerId).Column(ConnectivityDetails.GetDbFieldName("ServerId"));
            Map(x => x.ComponentOwner).Column(ConnectivityDetails.GetDbFieldName("ComponentOwner"));
            Map(x => x.Version).Column(ConnectivityDetails.GetDbFieldName("Version"));
            //TODO:restrictions composite key
        }
    }
}