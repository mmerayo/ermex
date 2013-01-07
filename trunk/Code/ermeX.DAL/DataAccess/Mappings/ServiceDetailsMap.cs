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
    internal abstract class ServiceDetailsMap : ClassMap<ServiceDetails>
    {
        protected abstract DbEngineType EngineType { get; }

        protected ServiceDetailsMap()
        {
            string tableName;
            switch (EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, ServiceDetails.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", ServiceDetails.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Table(tableName);
            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(ServiceDetails.GetDbFieldName("Id"));
            Map(x => x.ComponentOwner).Column(ServiceDetails.GetDbFieldName("ComponentOwner"));
            Map(x => x.OperationIdentifier).Column(ServiceDetails.GetDbFieldName("OperationIdentifier"));
            Map(x => x.ServiceImplementationMethodName).Column(
                ServiceDetails.GetDbFieldName("ServiceImplementationMethodName"));
            Map(x => x.ServiceImplementationTypeName).Column(
                ServiceDetails.GetDbFieldName("ServiceImplementationTypeName"));
            Map(x => x.Publisher).Column(ServiceDetails.GetDbFieldName("Publisher"));
            Map(x => x.Version).Column(ServiceDetails.GetDbFieldName("Version"));
            Map(x => x.ServiceInterfaceTypeName).Column(ServiceDetails.GetDbFieldName("ServiceInterfaceTypeName"));
            Map(x => x.IsSystemService).Column(ServiceDetails.GetDbFieldName("IsSystemService"));
        }
    }
}