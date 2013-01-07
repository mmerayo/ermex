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
    internal abstract class AppComponentMap : ClassMap<AppComponent>
    {
        protected abstract DbEngineType EngineType { get; }

        protected AppComponentMap()
        {
            string tableName;
            switch(EngineType)
            {
                case DbEngineType.SqlServer2008:
                    tableName = string.Format("{0}.{1}", DataSchemaType.ClientComponent, AppComponent.TableName);
                    break;
                case DbEngineType.Sqlite:
                    tableName = string.Format("{0}", AppComponent.TableName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Table( tableName);

            //LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column(AppComponent.GetDbFieldName("Id"));
            Map(x => x.ComponentId).Column(AppComponent.GetDbFieldName("ComponentId")).Unique();
            Map(x => x.ComponentOwner).Column(AppComponent.GetDbFieldName("ComponentOwner"));
            Map(x => x.Latency).Column(AppComponent.GetDbFieldName("Latency"));
            Map(x => x.Version).Column(AppComponent.GetDbFieldName("Version"));
            Map(x => x.IsRunning).Column(AppComponent.GetDbFieldName("IsRunning"));
            Map(x => x.ExchangedDefinitions).Column(AppComponent.GetDbFieldName("ExchangedDefinitions"));
            Map(x => x.ComponentExchanges).Column(AppComponent.GetDbFieldName("ComponentExchanges"));
            //TODO:restrictions composite key
        }
    }
}