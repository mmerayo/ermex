// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;

namespace ermeX.ConfigurationManagement.Settings
{
    internal interface IDalSettings
    {
        IList<DataSchemaType> SchemasApplied { get; }
        string ConfigurationConnectionString { get; }
        DbEngineType ConfigurationSourceType { get; }
    }
}
