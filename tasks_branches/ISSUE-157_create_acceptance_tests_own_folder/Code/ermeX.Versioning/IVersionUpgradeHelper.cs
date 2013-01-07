// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;

namespace ermeX.Versioning
{
    internal interface IVersionUpgradeHelper
    {
        void RunDataSchemaUpgrades(IList<DataSchemaType> schemasApplied, string configurationConnectionString,
                                   DbEngineType configurationSourceType);
    }
}