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

namespace ermeX.DAL.DataAccess.Mappings.SqlServer
{
    internal class ServiceDetailsMap : Mappings.ServiceDetailsMap
    {
        protected override DbEngineType EngineType { get { return DbEngineType.SqlServer2008; } }
    }
}