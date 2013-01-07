// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Versioning.Schema.Scripts;

namespace ermeX.Versioning
{
    internal class VersionUpgradeHelper : IVersionUpgradeHelper
    {
        #region IVersionUpgradeHelper Members

        public void RunDataSchemaUpgrades(IList<DataSchemaType> schemasApplied, string configurationConnectionString,
                                          DbEngineType configurationSourceType)
        {
            SchemaScriptRunner schemaScriptRunner = SchemaScriptRunner.GetRunner(schemasApplied,
                                                                                 configurationConnectionString,
                                                                                 configurationSourceType);
            schemaScriptRunner.RunUpgrades();
        }

        #endregion
    }
}