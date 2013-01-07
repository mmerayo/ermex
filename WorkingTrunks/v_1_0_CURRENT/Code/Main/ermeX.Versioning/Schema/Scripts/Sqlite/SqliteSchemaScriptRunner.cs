// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;

namespace ermeX.Versioning.Schema.Scripts.SqlServer
{
   
    internal sealed class SqliteSchemaScriptRunner : SchemaScriptRunner
    {
        private const string RexPrefx = "ermeX.Versioning.Schema.Scripts.Sqlite";

        public SqliteSchemaScriptRunner(IList<DataSchemaType> schemasApplied, string configurationConnectionString)
            : base(DbEngineType.Sqlite, schemasApplied, configurationConnectionString)
        {
        }


        protected override string NamespaceResourcesPrefix
        {
            get { return RexPrefx; }
        }

        protected override string GetLatestVersionExecutedSqlQuery(DataSchemaType dataSchemaType)
        {
            return string.Format(
                "Select Version_TimeStamp from [dbo].Version where Version_SchemaType={0} Order By Version_TimeStamp Desc LIMIT 1",
                (int) dataSchemaType);
        }
    }
}